using System.ComponentModel.DataAnnotations;

namespace AAI.MCP.Manufacturing.Options;

public class MachineDataOptions
{
    public const string MachineDataSettingsName = "MachineDataSettings";

    [Required(ErrorMessage = "Container name is required.")]
    public required string ContainerName { get; init; }

    [Required(ErrorMessage = "Connection string is required.")]
    public required string ConnectionString { get; init; }

    [Required(ErrorMessage = "Machine log file is required.")]
    public required string MachineLogFileName { get; set; } = "factory_log.txt";
}