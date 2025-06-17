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
}