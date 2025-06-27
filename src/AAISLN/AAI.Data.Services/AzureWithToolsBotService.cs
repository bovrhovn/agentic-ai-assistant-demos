using AAI.Interfaces;
using AAI.Models;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;

namespace AAI.Data.Services;

public class AzureWithToolsBotService(
    IChatRepository chatRepository,
    string projectEndpoint,
    string bingConnectionId,
    string deploymentURI,
    string modelDeploymentName)
    : IAgentWithToolsBotService
{
    public async Task<Chat> GetResponseAsync(string userInput, string conversationId, string userId, string parentId = "",string instructionsForAgent = "")
    {
        var defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeAzureCliCredential = false,
            ExcludeEnvironmentCredential = true,
            ExcludeInteractiveBrowserCredential = true,
            ExcludeVisualStudioCredential = true
        });
        PersistentAgentsClient client = new(projectEndpoint, defaultAzureCredential);
        var searchConfig = new BingGroundingSearchConfiguration(bingConnectionId)
        {
            Count = 5, Freshness = "Day"
        };

        var bingGroundingTool = new BingGroundingToolDefinition(
            new BingGroundingSearchToolParameters([searchConfig])
        );
        var instructions = "Use the bing grounding tool to answer questions. " +
                           "If the tool does not return any results, respond with 'I don't know'. ;";
        PersistentAgent agentWithBingTool = await client.Administration.CreateAgentAsync(
            model: modelDeploymentName,
            name: "AgentWithBingTool",
            instructions: instructions,
            tools: [bingGroundingTool]
        );

        var connectedAgentName = "agent_discussion_aai_with_tool";
        ConnectedAgentToolDefinition connectedAgentDefinition =
            new(new ConnectedAgentDetails(agentWithBingTool.Id, connectedAgentName,
                "Gets the stock price of a company"));

        var theWholeThread = await chatRepository.GetForThreadAsync(conversationId);
        
        PersistentAgent mainAgent = await client.Administration.CreateAgentAsync(
            model: modelDeploymentName,
            name: "aai_answer_bot",
            instructions: instructionsForAgent,
            tools: [connectedAgentDefinition]
        );

        PersistentAgentThread thread = await client.Threads.CreateThreadAsync();
        // give the history to the thread
        if (theWholeThread.Any())
        {
            foreach (var chat in theWholeThread)
            {
                 await client.Messages.CreateMessageAsync(
                    thread.Id,
                    chat.ChatType == ChatModelType.User ? MessageRole.User : MessageRole.Agent,
                    chat.Text
                );
            }
        }
        //give user input to the thread
        await client.Messages.CreateMessageAsync(thread.Id, 
            MessageRole.User, 
            userInput);
        ThreadRun run = await client.Runs.CreateRunAsync(thread, mainAgent);
        do
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(500));
            run = await client.Runs.GetRunAsync(thread.Id, run.Id);
        } while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress);

        if (run.Status != RunStatus.Completed)
            throw new Exception("Run did not complete successfully, error: " + run.LastError?.Message);

        Pageable<PersistentThreadMessage> messages = client.Messages.GetMessages(
            threadId: thread.Id,
            order: ListSortOrder.Ascending
        );
        var responseMessage = string.Empty;
        foreach (var threadMessage in messages)
        {
            foreach (var contentItem in threadMessage.ContentItems)
            {
                switch (contentItem)
                {
                    case MessageTextContent textItem:
                    {
                        var response = textItem.Text;
                        if (textItem.Annotations != null)
                        {
                            foreach (var annotation in textItem.Annotations)
                            {
                                if (annotation is MessageTextUriCitationAnnotation urlAnnotation)
                                {
                                    response = response.Replace(urlAnnotation.Text,
                                        $" [{urlAnnotation.UriCitation.Title}]({urlAnnotation.UriCitation.Uri})");
                                }
                            }
                        }

                        responseMessage = response;
                        break;
                    }
                }
            }
        }

        var model = new Chat
        {
            ChatId = Guid.NewGuid().ToString(),
            ThreadName = conversationId,
            UserId = userId,
            ParentId = parentId,
            Text = responseMessage,
            ChatType = ChatModelType.Assistant,
            DatePosted = DateTime.Now
        };
        await chatRepository.SaveAsync(model);
        return model;
    }
}