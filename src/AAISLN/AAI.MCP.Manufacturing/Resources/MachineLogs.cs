using System.ComponentModel;
using AAI.Interfaces;
using AAI.MCP.Manufacturing.Options;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace AAI.MCP.Manufacturing.Resources;

[McpServerResourceType]
public class MachineLogsFilesResources(
    ILogger<MachineLogsFilesResources> logger,
    IStorageService storageService,
    IOptions<MachineDataOptions> machineDataOptions)
{
    [McpServerResource(Name = "MachineLogs", Title = "Machine Log Resource", MimeType = "text/plain")]
    [Description("Returns a log of machine operations in a text format.")]
    public async Task<string> GetWeatherHistoryFromFile()
    {
        logger.LogInformation("Retrieving machine log file: {FileName}", machineDataOptions.Value.MachineLogFileName);
        var data = await storageService.GetFileContentAsync(machineDataOptions.Value.MachineLogFileName);
        if (string.IsNullOrEmpty(data))
        {
            logger.LogWarning("No data found in the machine log file: {FileName}", machineDataOptions.Value.MachineLogFileName);
            return "No data available.";
        }
        return data;
    }
}