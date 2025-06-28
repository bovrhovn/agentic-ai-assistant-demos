using Microsoft.Graph.Models.ExternalConnectors;

namespace AAI.GraphConnector.Data;

public static class CompanyMachineHelper
{
    public static Properties AsBasicExternalItemProperties(this CompanyMachineInfo model)
    {
        var properties = new Properties
        {
            AdditionalData = new Dictionary<string, object>
            {
                { "machineId", model.MachineId },
                { "name", model.Title },
                { "activated", model.Activated.ToUniversalTime() },
                { "restartCount", model.RestartCount },
                { "message", model.Message }
            }
        };

        return properties;
    }
}