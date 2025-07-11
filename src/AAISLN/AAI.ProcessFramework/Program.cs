﻿using AAI.ProcessFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

Console.WriteLine("Semantic Kernel Processes Demo");

#region Env variables

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

#region Process builder setup

ProcessBuilder process = new("ChatBot");
var introStep = process.AddStepFromType<IntroStep>();
var userInputStep = process.AddStepFromType<ChatUserInputStep>();
var responseStep = process.AddStepFromType<ChatBotResponseStep>();

#endregion

// Define the behavior when the process receives an external event
process
    .OnInputEvent(ChatBotEvents.StartProcess)
    .SendEventTo(new ProcessFunctionTargetBuilder(introStep));
// When the intro is complete, notify the userInput step
introStep
    .OnFunctionResult()
    .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));
// When the userInput step emits an exit event, send it to the end step
userInputStep
    .OnEvent(ChatBotEvents.Exit)
    .StopProcess();
// When the userInput step emits a user input event, send it to the assistantResponse step
userInputStep
    .OnEvent(CommonEvents.UserInputReceived)
    .SendEventTo(new ProcessFunctionTargetBuilder(responseStep, parameterName: "userMessage"));
// When the assistantResponse step emits a response, send it to the userInput step
responseStep
    .OnEvent(ChatBotEvents.AssistantResponseGenerated)
    .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

// Build the process to get a handle that can be started
var kernelProcess = process.Build();
await using var runningProcess =
    await kernelProcess.StartAsync(kernel, new KernelProcessEvent { Id = ChatBotEvents.StartProcess, Data = null });

namespace AAI.ProcessFramework
{
    sealed class IntroStep : KernelProcessStep
    {
        /// <summary>
        /// Prints an introduction message to the console.
        /// </summary>
        [KernelFunction]
        public void PrintIntroMessage()
        {
            Console.WriteLine("Welcome to Processes in Semantic Kernel.\n");
        }
    }

    sealed class ChatUserInputStep : ScriptedUserInputStep
    {
        protected override void PopulateUserInputs(UserInputState state)
        {
            state.UserInputs.Add("Hello");
            state.UserInputs.Add("How tall is the tallest mountain?");
            state.UserInputs.Add("How low is the lowest valley?");
            state.UserInputs.Add("How wide is the widest river?");
            state.UserInputs.Add("exit");
            state.UserInputs.Add("This text will be ignored because exit process condition was already met at this point.");
        }

        public override async ValueTask GetUserInputAsync(KernelProcessStepContext context)
        {
            var userMessage = GetNextUserMessage();

            if (string.Equals(userMessage, "exit", StringComparison.OrdinalIgnoreCase))
            {
                // exit condition met, emitting exit event
                await context.EmitEventAsync(new() { Id = ChatBotEvents.Exit, Data = userMessage });
                return;
            }

            // emitting userInputReceived event
            await context.EmitEventAsync(new() { Id = CommonEvents.UserInputReceived, Data = userMessage });
        }
    }

    /// <summary>
    /// A step that takes the user input from a previous step and generates a response from the chat completion service.
    /// </summary>
    sealed class ChatBotResponseStep : KernelProcessStep<ChatBotState>
    {
        public static class ProcessFunctions
        {
            public const string GetChatResponse = nameof(GetChatResponse);
        }

        /// <summary>
        /// The internal state object for the chat bot response step.
        /// </summary>
        internal ChatBotState? _state;

        /// <summary>
        /// ActivateAsync is the place to initialize the state object for the step.
        /// </summary>
        /// <param name="state">An instance of <see cref="ChatBotState"/></param>
        /// <returns>A <see cref="ValueTask"/></returns>
        public override ValueTask ActivateAsync(KernelProcessStepState<ChatBotState> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Generates a response from the chat completion service.
        /// </summary>
        /// <param name="context">The context for the current step and process. <see cref="KernelProcessStepContext"/></param>
        /// <param name="userMessage">The user message from a previous step.</param>
        /// <param name="_kernel">A <see cref="Kernel"/> instance.</param>
        /// <returns></returns>
        [KernelFunction(ProcessFunctions.GetChatResponse)]
        public async Task GetChatResponseAsync(KernelProcessStepContext context, string userMessage, Kernel _kernel)
        {
            _state!.ChatMessages.Add(new(AuthorRole.User, userMessage));
            var chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();
            var response =
                await chatService.GetChatMessageContentAsync(_state.ChatMessages).ConfigureAwait(false);
            if (response == null)
            {
                throw new InvalidOperationException("Failed to get a response from the chat completion service.");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"ASSISTANT: {response.Content}");
            Console.ResetColor();

            // Update state with the response
            _state.ChatMessages.Add(response);

            // emit event: assistantResponse
            await context.EmitEventAsync(new KernelProcessEvent
                { Id = ChatBotEvents.AssistantResponseGenerated, Data = response });
        }
    }

    /// <summary>
    /// The state object for the <see cref="ChatBotResponseStep"/>.
    /// </summary>
    sealed class ChatBotState
    {
        internal ChatHistory ChatMessages { get; } = new();
    }

    /// <summary>
    /// A class that defines the events that can be emitted by the chat bot process. This is
    /// not required but used to ensure that the event names are consistent.
    /// </summary>
    static class ChatBotEvents
    {
        public const string StartProcess = "startProcess";
        public const string IntroComplete = "introComplete";
        public const string AssistantResponseGenerated = "assistantResponseGenerated";
        public const string Exit = "exit";
    }
}