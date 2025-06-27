using AAI.Interfaces;
using AAI.Models;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

namespace AAI.Data.Services;

public class AgenToMCPBotService(IChatRepository chatRepository,
    string mcpEndpoint,
    string deploymentURI, 
    string deploymentName,
    string apiKey) 
    : IAgentToMcpBotService
{
    public async Task<Chat> GetResponseAsync(string userInput, string conversationId, string userId, string parentId = "",
        string instructionsForAgent = "")
    {
        var theWholeThread = await chatRepository.GetForThreadAsync(conversationId);
        if (theWholeThread.Any()) parentId = theWholeThread.Last().ChatId;
        
        var toolName = "ManufacturingAAI";
        var transport = new SseClientTransport(new SseClientTransportOptions
        {
            Name = toolName,
            Endpoint = new Uri(mcpEndpoint)
        });

        var mcpClient = await McpClientFactory.CreateAsync(transport);
        var tools = await mcpClient.ListToolsAsync();
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(deploymentName, deploymentURI, apiKey);
        var kernel = builder.Build();
        var kernelFunctions = tools.Select(tool => tool.AsKernelFunction());
        kernel.Plugins.AddFromFunctions(toolName, kernelFunctions);
        var result = await kernel.InvokePromptAsync("Use the tool to get the answer to the question: " + userInput);
        var mcpResponse = result.GetValue<string>();
        ArgumentException.ThrowIfNullOrEmpty(mcpResponse);
        
        var model = new Chat
        {
            ChatId = Guid.NewGuid().ToString(),
            ThreadName = conversationId,
            UserId = userId,
            ParentId = parentId,
            Text = mcpResponse,
            ChatType = ChatModelType.Assistant,
            DatePosted = DateTime.Now
        };
        
        await chatRepository.SaveAsync(model);
        return model;
    }
}