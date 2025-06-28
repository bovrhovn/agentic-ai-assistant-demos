using AAI.Core;
using AAI.MCP.Manufacturing.Options;
using AAI.MCP.Manufacturing.Tools;
using Microsoft.Extensions.Options;

namespace AAI.MCP.Manufacturing.Services;

public class MachineService(
    ILogger<MachineService> logger,
    IOptions<WebOptions> webDataOptions,
    HttpClient httpClient)
{
    public async Task<List<MachineInfo>> GetMachinesAsync()
    {
        try
        {
            var url =
                $"{webDataOptions.Value.BaseUrl}{DataRoutes.ManufacturingRoute}/{DataRoutes.ManufacturingGetMachinesRoute}";
            logger.LogInformation("Fetching machines from route: {Route}", url);
            var response = await httpClient.GetStringAsync(url);
            var machineList = System.Text.Json.JsonSerializer.Deserialize<List<MachineInfo>>(response);
            logger.LogInformation("Fetched {Count} machines", machineList?.Count ?? 0);
            return machineList ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get machines");
            return [];
        }
    }

    public async Task<List<FinishedGoodInfo>> GetFinishedGoodsAsync()
    {
        try
        {
            var url =
                $"{webDataOptions.Value.BaseUrl}{DataRoutes.ManufacturingRoute}/{DataRoutes.ManufacturingGetMachinesRoute}";
            logger.LogInformation("Fetching machines from route: {Route}", url);
            var response = await httpClient.GetStringAsync(url);
            var finishedGoods = System.Text.Json.JsonSerializer.Deserialize<List<FinishedGoodInfo>>(response);
            logger.LogInformation("Fetched {Count} finished goods", finishedGoods?.Count ?? 0);
            return finishedGoods ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get machines");
            return [];
        }
    }
}