﻿#region Usings

using System.Text.Json;
using AAI.MakerChecker;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

#endregion

#region Environment Variables

Console.WriteLine("Maker Agents pattern with Semantic Kernel...");

var deploymentName = Environment.GetEnvironmentVariable("DEPLOYMENTNAME") ?? "gpt-4o";
ArgumentException.ThrowIfNullOrEmpty(deploymentName);
var apiKey = Environment.GetEnvironmentVariable("APIKEY");
ArgumentException.ThrowIfNullOrEmpty(apiKey);
var endpoint = Environment.GetEnvironmentVariable("ENDPOINTURL");
ArgumentException.ThrowIfNullOrEmpty(endpoint);

#endregion

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
var kernel = builder.Build();

var toolKernel = kernel.Clone();
toolKernel.Plugins.AddFromType<ClipboardAccess>();

const string ReviewerName = "Reviewer";
const string WriterName = "Writer";

ChatCompletionAgent agentReviewer =
    new()
    {
        Name = ReviewerName,
        Instructions =
            """
            Your responsibility is to review and identify how to improve user provided content.
            If the user has providing input or direction for content already provided, specify how to address this input.
            Never directly perform the correction or provide example.
            Once the content has been updated in a subsequent response, you will review the content again until satisfactory.
            Always copy satisfactory content to the clipboard using available tools and inform user.

            RULES:
            - Only identify suggestions that are specific and actionable.
            - Verify previous suggestions have been addressed.
            - Never repeat previous suggestions.
            """,
        Kernel = toolKernel,
        Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings
            { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
    };

ChatCompletionAgent agentWriter =
    new()
    {
        Name = WriterName,
        Instructions =
            """
            Your sole responsibility is to rewrite content according to review suggestions.

            - Always apply all review direction.
            - Always revise the content in its entirety without explanation.
            - Never address the user.
            """,
        Kernel = kernel,
    };

var selectionFunction =
    AgentGroupChat.CreatePromptFunctionForStrategy(
        $$$"""
           Examine the provided RESPONSE and choose the next participant.
           State only the name of the chosen participant without explanation.
           Never choose the participant named in the RESPONSE.

           Choose only from these participants:
           - {{{ReviewerName}}}
           - {{{WriterName}}}

           Always follow these rules when choosing the next participant:
           - If RESPONSE is user input, it is {{{ReviewerName}}}'s turn.
           - If RESPONSE is by {{{ReviewerName}}}, it is {{{WriterName}}}'s turn.
           - If RESPONSE is by {{{WriterName}}}, it is {{{ReviewerName}}}'s turn.

           RESPONSE:
           {{$lastmessage}}
           """,
        safeParameterNames: "lastmessage");

const string TerminationToken = "yes";

var terminationFunction =
    AgentGroupChat.CreatePromptFunctionForStrategy(
        $$$"""
           Examine the RESPONSE and determine whether the content has been deemed satisfactory.
           If content is satisfactory, respond with a single word without explanation: {{{TerminationToken}}}.
           If specific suggestions are being provided, it is not satisfactory.
           If no correction is suggested, it is satisfactory.

           RESPONSE:
           {{$lastmessage}}
           """,
        safeParameterNames: "lastmessage");

ChatHistoryTruncationReducer historyReducer = new(1);

AgentGroupChat chat =
    new(agentReviewer, agentWriter)
    {
        ExecutionSettings = new AgentGroupChatSettings
        {
            SelectionStrategy =
                new KernelFunctionSelectionStrategy(selectionFunction, kernel)
                {
                    // Always start with the editor agent.
                    InitialAgent = agentReviewer,
                    // Save tokens by only including the final response
                    HistoryReducer = historyReducer,
                    // The prompt variable name for the history argument.
                    HistoryVariableName = "lastmessage",
                    // Returns the entire result value as a string.
                    ResultParser = result => result.GetValue<string>() ?? agentReviewer.Name
                },
            TerminationStrategy =
                new KernelFunctionTerminationStrategy(terminationFunction, kernel)
                {
                    // Only evaluate for editor's response
                    Agents = [agentReviewer],
                    // Save tokens by only including the final response
                    HistoryReducer = historyReducer,
                    // The prompt variable name for the history argument.
                    HistoryVariableName = "lastmessage",
                    // Limit total number of turns
                    MaximumIterations = 12,
                    // Customer result parser to determine if the response is "yes"
                    ResultParser = result =>
                        result.GetValue<string>()?.Contains(TerminationToken, StringComparison.OrdinalIgnoreCase) ??
                        false
                }
        }
    };

Console.WriteLine("Ready to start with reviews!");
//set the base path for file access
var basePath = Environment.GetEnvironmentVariable("BASEPATH");
ArgumentException.ThrowIfNullOrEmpty(basePath);
var isComplete = false;
do
{
    Console.WriteLine();

    #region CLI menu

    Console.Write("> ");
    var input = Console.ReadLine() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(input))
        continue;

    input = input.Trim();
    if (input.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        isComplete = true;
        break;
    }

    if (input.Equals("RESET", StringComparison.OrdinalIgnoreCase))
    {
        await chat.ResetAsync();
        Console.WriteLine("[Conversation has been reset]");
        continue;
    }

    if (input.StartsWith("@", StringComparison.Ordinal) && input.Length >= 1)
    {
        var filePath = input.Length == 1
            ? Path.Join(basePath, "test.txt")
            : Path.Join(basePath, input.Substring(1));
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Unable to access file: {filePath}");
                continue;
            }

            input = File.ReadAllText(filePath);
        }
        catch (Exception)
        {
            Console.WriteLine($"Unable to access file: {filePath}");
            continue;
        }
    }

    #endregion

    chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
    chat.IsComplete = false; //chat was not complete, so reset the flag

    try
    {
        await foreach (var response in chat.InvokeAsync())
        {
            Console.WriteLine();
            Console.WriteLine($"{response.AuthorName?.ToUpperInvariant()}:{Environment.NewLine}{response.Content}");
        }
    }
    catch (HttpOperationException exception)
    {
        Console.WriteLine(exception.Message);
        if (exception.InnerException != null)
        {
            Console.WriteLine(exception.InnerException.Message);
            if (exception.InnerException.Data.Count > 0)
            {
                Console.WriteLine(JsonSerializer.Serialize(exception.InnerException.Data,
                    new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
} while (!isComplete);

Console.WriteLine("Exiting...");