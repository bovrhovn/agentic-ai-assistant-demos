namespace AAI.Core;

public record BotMode(int BotModeId, string Name)
{
    public static BotMode Default => AzureOpenAIDeployment;

    public static BotMode FromId(int botModeId) =>
        botModeId switch
        {
            1 => AzureOpenAIDeployment,
            2 => AgentWithTools,
            3 => AgentToAgent,
            4 => AgentToAgentWithTools,
            5 => AgentToMcp,
            _ => Default
        };

    public static List<BotMode> GetAll() =>
    [
        AzureOpenAIDeployment,
        AgentWithTools,
        AgentToAgent,
        AgentToAgentWithTools,
        AgentToMcp
    ];
    
    public static BotMode AzureOpenAIDeployment { get; } = new(1, "Azure OpenAI Deployment");
    public static BotMode AgentWithTools { get; } = new(2, "Agent with tools");
    public static BotMode AgentToAgent { get; } = new(3, "Agent to Agent");
    public static BotMode AgentToAgentWithTools { get; } = new(4, "Agent to Agent with tools");
    public static BotMode AgentToMcp { get; } = new(5, "Agent to MCP");
}
