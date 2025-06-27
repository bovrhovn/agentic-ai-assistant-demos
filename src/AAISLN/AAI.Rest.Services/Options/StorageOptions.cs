using System.ComponentModel.DataAnnotations;

namespace AAI.Rest.Services.Options;

public class MachineStorageOptions
{
    public const string StorageSettingsName = "MachineStorageOptions";
    [Required(ErrorMessage = "Storage URI is required settings")]
    public required string StorageUri { get; init; }

    [Required(ErrorMessage = "Log container name is required settings")]
    public required string LogContainer { get; init; }
}