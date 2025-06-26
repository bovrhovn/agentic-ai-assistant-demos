using System.ComponentModel;
using AAI.Core;
using AAI.MCP.Manufacturing.Options;
using AAI.MCP.Manufacturing.Services;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace AAI.MCP.Manufacturing.Tools;

[McpServerToolType,
 Description("This tool provides information about manufacturing processes and machines.")]
public class ManufacturingTool(ILogger<ManufacturingTool> logger, MachineService machineService)
{
    [McpServerTool(Name = "machines", Title = "Machine list tool")]
    [Description("Returns machines in the manufacturing.")]
    public async Task<List<MachineInfo>> GetMachines() => await machineService.GetMachinesAsync();

    [McpServerTool(Name = "manhealth", Title = "Health check tool")]
    [Description("Returns status about the tool.")]
    public async Task<bool> CheckHealthForTools(HttpClient client, IOptions<WebOptions> options)
    {
        client.BaseAddress = new Uri(options.Value.BaseUrl);
        try
        {
            var response = await client.GetAsync(GeneralRoutes.HealthRoute);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

public record MachineInfo(string machineId, string status, double temperature);