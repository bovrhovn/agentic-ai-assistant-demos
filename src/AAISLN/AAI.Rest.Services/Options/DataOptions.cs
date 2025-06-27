using System.ComponentModel.DataAnnotations;

namespace AAI.Rest.Services.Options;

public class DataOptions
{
    public const string DataSettingsName = "DataOptions";

    [Required(ErrorMessage = "Connection string to data store is required settings")]
    public required string ConnectionString { get; init; }

    [Required(ErrorMessage = "Chat container name is required settings")]
    public required string ChatContainer { get; init; }

    public string DatabaseName { get; init; } = "aaidb";

    [Required(ErrorMessage = "Agent project URI is required settings")]
    public required string AgentsProjectUri { get; init; }

    [Required(ErrorMessage = "Azure OpenAI base URI is required settings")]
    public required string AzureOpenAIBaseURI { get; init; }
    
    public string ManufacturingLogFileName { get; init; } = "factory_log.txt";

    [Required(ErrorMessage = "Deployment name is required settings")]
    public required string DeploymentName { get; init; } = "gpt-4o";
}