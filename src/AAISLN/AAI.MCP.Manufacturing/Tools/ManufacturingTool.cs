using System.ComponentModel;
using AAI.Core;
using AAI.Data.Services;
using AAI.MCP.Manufacturing.Options;
using AAI.MCP.Manufacturing.Services;
using AAI.Models;
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
    
    [McpServerTool(Name = "finishedGoods", Title = "Finished goods tool")]
    [Description("Retrieves a list of finished goods produced in the manufacturing environment")]
    public async Task<List<FinishedGoodInfo>> GetFinishedGoods() => await machineService.GetFinishedGoodsAsync();

    [McpServerTool(Name = "machineStatusList", Title = "Machine status list tool")]
    [Description("Retrieves a list of machine statuses in the manufacturing environment")]
    public List<string> GetMachineStatusesList() => FakeDataGenerator.GetMachineStatuses();

    [McpServerTool(Name = "manhealth", Title = "Health check tool")]
    [Description("Returns status about the tool.")]
    public async Task<bool> CheckHealthForTools(HttpClient client, IOptions<WebOptions> options)
    {
        logger.LogInformation("Checking health for manufacturing tools...");
        client.BaseAddress = new Uri(options.Value.BaseUrl);
        try
        {
            var response = await client.GetAsync(GeneralRoutes.HealthRoute);
            return response.IsSuccessStatusCode;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error checking health for manufacturing tools.");
            return false;
        }
    }
}