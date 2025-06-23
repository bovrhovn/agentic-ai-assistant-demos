using System.ComponentModel;
using AAI.Core;
using AAI.MCP.Manufacturing.Options;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace AAI.MCP.Manufacturing.Tools;

[McpServerToolType,
 Description("This tool provides information about manufacturing processes and machines.")]
public static class ManufacturingTool
{
    [McpServerTool(Name = "machines", Title = "Machine list tool")]
    [Description("Returns machines in the manufacturing.")]
    public static async Task<List<MachineInfo>> GetMachines(HttpClient client, IOptions<WebOptions> options)
    {
        client.BaseAddress = new Uri(options.Value.BaseUrl);
        var machines = await client.GetStringAsync(DataRoutes.ManufacturingGetMachinesRoute);
        var machineList = System.Text.Json.JsonSerializer.Deserialize<List<MachineInfo>>(machines);
        return machineList ?? [];
    }
}

public record MachineInfo(string machineId, string status, double temperature);