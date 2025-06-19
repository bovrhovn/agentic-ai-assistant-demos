using System.ComponentModel.DataAnnotations;

namespace AAI.Core;

public class StorageOptions
{
    public const string AppSettingsName = "StorageOptions";
    [Required(ErrorMessage = "Connection string to storage is required settings")]
    public required string ConnectionString { get; init; }
    [Required(ErrorMessage = "Settings container name is required settings")]
    public required string SettingsContainer { get; init; }
    public string DatabaseName { get; init; } = "aaidb";
}