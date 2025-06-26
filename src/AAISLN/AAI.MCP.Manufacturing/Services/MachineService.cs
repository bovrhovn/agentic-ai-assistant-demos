using AAI.Core;
using AAI.MCP.Manufacturing.Options;
using AAI.MCP.Manufacturing.Tools;
using Microsoft.Extensions.Options;

namespace AAI.MCP.Manufacturing.Services;

public class MachineService(ILogger<MachineService> logger, IOptions<WebOptions> webDataOptions, 
    HttpClient httpClient)
{
    public async Task<List<MachineInfo>> GetMachinesAsync()
    {
        try
        {
            httpClient.BaseAddress = new Uri(webDataOptions.Value.BaseUrl);
            var response = await httpClient.GetStringAsync(DataRoutes.ManufacturingGetMachinesRoute);
            var machineList = System.Text.Json.JsonSerializer.Deserialize<List<MachineInfo>>(response);
            return machineList ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get machines");
            return [];
        }
    }
}