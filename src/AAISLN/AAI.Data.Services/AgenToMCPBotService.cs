using System.ComponentModel;
using System.Diagnostics;
using AAI.Interfaces;
using AAI.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

namespace AAI.Data.Services;

public class AgenToMCPBotService(
    IChatRepository chatRepository,
    string mcpEndpoint,
    string deploymentURI,
    string deploymentName,
    string apiKey)
    : IAgentToMcpBotService
{
    public async Task<Chat> GetResponseAsync(string userInput, string conversationId, string userId,
        string parentId = "",
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

        //kernel.Plugins.AddFromType<ClipboardAccess>();
        var kernelFunctions = tools.Select(tool => tool.AsKernelFunction());
        kernel.Plugins.AddFromFunctions(toolName, kernelFunctions);

        var executionSettings = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = await kernel.InvokePromptAsync(
            $"Use the tool to get the answer to the question: {userInput}." +
            "Do a pretty formatted response with the tool name and the answer.",
            new(executionSettings));
        
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

    sealed class ClipboardAccess
    {
        [KernelFunction]
        [Description("Copies the provided content to the clipboard.")]
        public static void SetClipboard(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            using var clipProcess = Process.Start(
                new ProcessStartInfo
                {
                    FileName = "clip",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                }) ?? throw new InvalidOperationException("Unable to start clip process.");

            clipProcess.StandardInput.Write(content);
            clipProcess.StandardInput.Close();
        }
    }
}