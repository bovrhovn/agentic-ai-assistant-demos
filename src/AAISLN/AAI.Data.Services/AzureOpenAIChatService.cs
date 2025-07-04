﻿using AAI.Interfaces;
using AAI.Models;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Chat;

namespace AAI.Data.Services;

public class AzureOpenAIChatService : IAzureOpenAIBotService
{
    private readonly IChatRepository chatRepository;
    private readonly string deploymentName;
    private readonly AzureOpenAIClient client;

    public AzureOpenAIChatService(IChatRepository chatRepository, string deploymentURI, string deploymentName)
    {
        this.chatRepository = chatRepository;
        this.deploymentName = deploymentName;
        var defaultAzureCredential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ExcludeAzureCliCredential = false,
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = false,
                ExcludeVisualStudioCredential = true
            });
        client = new(new Uri(deploymentURI), defaultAzureCredential);
    }

    public async Task<Chat> GetResponseAsync(string userInput, string conversationId, string userId,
        string parentId = "", string instructionsForAgent = "")
    {
        var chatClient = client.GetChatClient(deploymentName);
        var requestOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 4096,
            Temperature = 1.0f,
            TopP = 1.0f
        };
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a chat assistant that helps users with their queries. " +
                                  "You should provide helpful and accurate responses based on the user's input." +
                                  "If you don't know the answer say 'I don't know'.")
        };
        var theWholeThread = await chatRepository.GetForThreadAsync(conversationId);
        if (theWholeThread.Any())
        {
            //add all previous messages to the chat
            foreach (var chat in theWholeThread)
            {
                switch (chat.ChatType)
                {
                    case ChatModelType.Assistant:
                        messages.Add(new AssistantChatMessage(chat.Text));
                        break;
                    case ChatModelType.System:
                        messages.Add(new SystemChatMessage(chat.Text));
                        break;
                    case ChatModelType.User:
                        messages.Add(new UserChatMessage(chat.Text));
                        break;
                }
            }

            parentId = theWholeThread.Last().ChatId;
        }

        var response = await chatClient.CompleteChatAsync(messages, requestOptions);
        if (response.Value == null)
            throw new InvalidOperationException("No response from the chat service.");

        var assistantMessage = response.Value.Content.FirstOrDefault()?.Text;
        if (string.IsNullOrEmpty(assistantMessage))
            throw new InvalidOperationException("No content in the response from the chat service.");

        //call to Azure OpenAI and save the response to the chat repository
        var model = new Chat
        {
            ChatId = Guid.NewGuid().ToString(),
            ThreadName = conversationId,
            UserId = userId,
            ParentId = parentId,
            Text = assistantMessage,
            ChatType = ChatModelType.Assistant,
            DatePosted = DateTime.Now
        };
        //save to history and return the model
        // OPTIMIZE: send to queue for async processing
        await chatRepository.SaveAsync(model);
        return model;
    }
}