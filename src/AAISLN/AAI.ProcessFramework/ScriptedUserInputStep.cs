﻿using Microsoft.SemanticKernel;

namespace AAI.ProcessFramework;

public class ScriptedUserInputStep : KernelProcessStep<UserInputState>
{
    public static class ProcessStepFunctions
    {
        public const string GetUserInput = nameof(GetUserInput);
    }

    protected bool SuppressOutput { get; init; }

    /// <summary>
    /// The state object for the user input step. This object holds the user inputs and the current input index.
    /// </summary>
    private UserInputState? state;

    /// <summary>
    /// Method to be overridden by the user to populate with custom user messages
    /// </summary>
    /// <param name="state">The initialized state object for the step.</param>
    protected virtual void PopulateUserInputs(UserInputState state)
    {
    }

    /// <summary>
    /// Activates the user input step by initializing the state object. This method is called when the process is started
    /// and before any of the KernelFunctions are invoked.
    /// </summary>
    /// <param name="state">The state object for the step.</param>
    /// <returns>A <see cref="ValueTask"/></returns>
    public override ValueTask ActivateAsync(KernelProcessStepState<UserInputState> state)
    {
        this.state = state.State;
        PopulateUserInputs(this.state!);
        return ValueTask.CompletedTask;
    }

    internal string GetNextUserMessage()
    {
        if (state != null && state.CurrentInputIndex >= 0 && state.CurrentInputIndex < state.UserInputs.Count)
        {
            var userMessage = state!.UserInputs[state.CurrentInputIndex];
            state.CurrentInputIndex++;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"USER: {userMessage}");
            Console.ResetColor();

            return userMessage;
        }

        Console.WriteLine("SCRIPTED_USER_INPUT: No more scripted user messages defined, returning empty string as user message");
        return string.Empty;
    }

    /// <summary>
    /// Gets the user input.
    /// Could be overridden to customize the output events to be emitted
    /// </summary>
    /// <param name="context">An instance of <see cref="KernelProcessStepContext"/> which can be
    /// used to emit events from within a KernelFunction.</param>
    /// <returns>A <see cref="ValueTask"/></returns>
    [KernelFunction(ProcessStepFunctions.GetUserInput)]
    public virtual async ValueTask GetUserInputAsync(KernelProcessStepContext context)
    {
        var userMessage = GetNextUserMessage();
        // Emit the user input
        if (string.IsNullOrEmpty(userMessage))
        {
            await context.EmitEventAsync(new() { Id = CommonEvents.Exit });
            return;
        }

        await context.EmitEventAsync(new() { Id = CommonEvents.UserInputReceived, Data = userMessage });
    }
}

/// <summary>
/// The state object for the <see cref="ScriptedUserInputStep"/>
/// </summary>
public record UserInputState
{
    public List<string> UserInputs { get; } = [];
    public int CurrentInputIndex { get; set; } = 0;
}