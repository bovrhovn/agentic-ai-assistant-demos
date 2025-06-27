using System.ComponentModel.DataAnnotations;

namespace AAI.Core;

public class DataStorageOptions
{
    public const string DataStorageOptionsName = "DataStorageOptions";
    [Required(ErrorMessage = "Connection string to storage is required settings")]
    public required string ConnectionString { get; init; }
    [Required(ErrorMessage = "Settings container name is required settings")]
    public required string SettingsContainer { get; init; }
    public string DatabaseName { get; init; } = "aaidb";
}