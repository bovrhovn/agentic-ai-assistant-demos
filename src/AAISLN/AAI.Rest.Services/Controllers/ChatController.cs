﻿using System.Net.Mime;
using AAI.Core;
using AAI.Interfaces;
using AAI.Models;
using AAI.Rest.Services.DtoModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AAI.Rest.Services.Controllers;

[ApiController, Route(GeneralRoutes.ChatRoute),
 AllowAnonymous, Produces(MediaTypeNames.Application.Json)]
public class ChatController(
    ILogger<ChatController> logger,
    IChatRepository chatRepository,
    ISettingsService settingsService,
    IAzureOpenAIBotService azureOpenAIBotService,
    IAgentWithToolsBotService agentWithToolsBotService,
    IAgentToMcpBotService agentToMcpBotService,
    IAgentToAgentBotService agentToAgentService)
    : ControllerBase
{
    private record ChatItem(string id, string parentId, string text, string sender, string timeStamp);

    [HttpPost]
    [Route(DataRoutes.SaveChatRoute)]
    [EndpointSummary("Save chat to the repository.")]
    [EndpointDescription(
        "This endpoint is used to save a chat to the repository. It is used by the AAI chat interface to save the chat history.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> SaveChatAsync([FromBody] ChatDto chatDto)
    {
        logger.LogInformation("Called save endpoint at {DateCalled}", DateTime.UtcNow);
        //saving user sent chat to history
        var chat = new Chat
        {
            ChatId = Guid.NewGuid().ToString(),
            UserId = chatDto.Email,
            ThreadName = chatDto.ThreadName,
            Text = chatDto.Text,
            ParentId = chatDto.ParentId,
            DatePosted = DateTime.Now
        };
        try
        {
            await chatRepository.SaveAsync(chat);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save chat for user {Email} at {DateCalled}", chatDto.Email, DateTime.UtcNow);
            return BadRequest($"Failed to save chat at {DateTime.Now}.");
        }

        logger.LogInformation("Chat saved successfully for user {Email} at {DateSaved}", chatDto.Email,
            DateTime.UtcNow);

        try
        {
            var settings = await settingsService.GetAsync(chatDto.Email);
            var instructions = string.Empty;
            IBotService botService;
            if (settings.BotMode == BotMode.AzureOpenAIDeployment)
                botService = azureOpenAIBotService;
            else if (settings.BotMode == BotMode.AgentToMcp)
                botService = agentToMcpBotService;
            else if (settings.BotMode == BotMode.AgentWithTools)
            {
                botService = agentWithToolsBotService;
                instructions =
                    "You are an AI assistant that helps users with their questions. Answer the question as best as you can.";
            }
            else if (settings.BotMode == BotMode.AgentToAgent)
            {
                botService = agentToAgentService;
                instructions =
                    "You are an AI assistant that helps users with their questions. Answer the question as best as you can.";
            }
            else
                throw new InvalidOperationException("Invalid bot mode specified.");

            //get answer from the bot service, save it to the chat repository and return the data
            var botMessage = await botService.GetResponseAsync(chatDto.Text, chatDto.ThreadName, chatDto.Email,
                instructionsForAgent: instructions);
            logger.LogInformation("Chat saved successfully at {DateSaved}", DateTime.UtcNow);
            return Ok(new[]
            {
                new ChatItem(chat.ChatId,
                    chat.ParentId,
                    chat.Text,
                    chat.ChatType.ToString().ToLowerInvariant(),
                    chat.DatePosted.ToString("o")),
                new ChatItem(botMessage.ChatId,
                    botMessage.ParentId,
                    botMessage.Text,
                    botMessage.ChatType.ToString().ToLowerInvariant(),
                    botMessage.DatePosted.ToString("o"))
            });
        }
        catch (Exception e)
        {
            logger.LogWarning(
                "Bot response is empty for user {Email} at {DateCalled} with error message {ErrorMessage}",
                chatDto.Email, DateTime.UtcNow, e.Message);
            return BadRequest($"Failed to get bot response at {DateTime.Now} with {e.Message}.");
        }
    }

    [HttpGet]
    [Route(DataRoutes.GetHistoryRoute + "/{email}")]
    [EndpointSummary("Get history chats for user based on primary key.")]
    [EndpointDescription(
        "This endpoint is used to get the chat history for a user based on their email address. It is used by the AAI chat interface to display the chat history.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistoryAsync([FromRoute] string email)
    {
        logger.LogInformation("Called get history endpoint at {DateCalled}", DateTime.UtcNow);
        var items = await chatRepository.GetForUserAsync(email);
        if (items == null || items.Count == 0)
        {
            logger.LogInformation("No chat history found for user {Email} at {DateCalled}", email, DateTime.UtcNow);
            return NotFound($"No chat history found for user {email}.");
        }

        logger.LogInformation("Found {Count} items for {Email}", items.Count, email);
        return Ok(items);
    }

    [HttpGet]
    [Route(DataRoutes.GetThreadDataRoute + "/{threadName}")]
    [EndpointSummary("Get thread items for user based on thread name.")]
    [EndpointDescription(
        "This endpoint is used to get the chat history for a user based on their thread name. It is used by the AAI chat interface to display the chat history for a specific thread.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThreadItemsAsync(string threadName)
    {
        logger.LogInformation("Called get items for thread endpoint at {DateCalled}", DateTime.UtcNow);
        var items = await chatRepository.GetForThreadAsync(threadName);
        logger.LogInformation("Found {Count} items for {ThreadName}", items.Count, threadName);
        var list = new List<ChatItem>();
        foreach (var item in items)
        {
            list.Add(new ChatItem(item.ChatId,
                item.ParentId,
                item.Text,
                item.ChatType.ToString().ToLowerInvariant(),
                item.DatePosted.ToString("o")));
        }

        return Ok(list);
    }

    [HttpGet]
    [Route(DataRoutes.GenerateThreadNameRoute)]
    [EndpointSummary("Generate a new thread name.")]
    [EndpointDescription(
        "This endpoint is used to generate a new thread name for the chat. It is used by the AAI chat interface to create a new thread.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GenerateNewThreadName()
    {
        logger.LogInformation("Called generate new thread endpoint at {DateCalled}", DateTime.UtcNow);
        return Ok(StringHelper.GenerateUniqueName());
    }

    [HttpGet]
    [Route(GeneralRoutes.HealthRoute)]
    [EndpointSummary("This is a health check for the chat controller.")]
    [EndpointDescription(
        "This is a health check for the AAI chat controller to see if it is online and all system are workings.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult IsAlive()
    {
        logger.LogInformation("Called alive data endpoint fpr chat controller at {DateCalled}", DateTime.UtcNow);
        return new ContentResult
            { StatusCode = 200, Content = $"Chat controller is alive at {DateTime.Now} on {Environment.MachineName}" };
    }
}