using AAI.Interfaces;
using AAI.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace AAI.Data.Services;

public class AgentToAgentService(
    IChatRepository chatRepository,
    string deploymentURI,
    string deploymentName,
    string apiKey) : IAgentToAgentBotService
{
    public async Task<Chat> GetResponseAsync(string userInput, string conversationId, string userId,
        string parentId = "",
        string instructionsForAgent = "")
    {
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(deploymentName, deploymentURI, apiKey);
        var kernel = builder.Build();

        var toolKernel = kernel.Clone();
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
                                result.GetValue<string>()
                                    ?.Contains(TerminationToken, StringComparison.OrdinalIgnoreCase) ??
                                false
                        }
                }
            };

        var theWholeThread = await chatRepository.GetForThreadAsync(conversationId).ConfigureAwait(false);
        //get the whole thread of messages to be added to the chat for history purposes
        if (theWholeThread.Any())
        {
            //add all previous messages to the chat
            foreach (var currentChat in theWholeThread)
            {
                switch (currentChat.ChatType)
                {
                    case ChatModelType.Assistant:
                        chat.AddChatMessage(new ChatMessageContent(AuthorRole.Assistant, currentChat.Text));
                        break;
                    case ChatModelType.System:
                        chat.AddChatMessage(new ChatMessageContent(AuthorRole.System, currentChat.Text));
                        break;
                    case ChatModelType.User:
                        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, currentChat.Text));
                        break;
                }
            }
        }

        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));
        var mcpResponse = string.Empty;

        await foreach (var response in chat.InvokeAsync().ConfigureAwait(false))
        {
            mcpResponse += response.Content;
        }

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

        await chatRepository.SaveAsync(model).ConfigureAwait(false);
        return model;
    }
}