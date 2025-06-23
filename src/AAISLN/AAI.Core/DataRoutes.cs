namespace AAI.Core;

public static class DataRoutes
{
    //chat routes
    public const string SaveChatRoute = "save";
    public const string GenerateThreadNameRoute = "generate-thread-name";
    public const string GetHistoryRoute = "get-history";
    public const string GetThreadDataRoute = "get-thread-data";
    //manufacturing routes
    public const string ManufacturingRoute = "manufacturing";
    public const string ManufacturingGetMachinesRoute = "get-machines";
    public const string ManufacturingGetMachineStatusRoute = "get-machine-status";
    public const string ManufacturingGetRawMaterialsRoute = "get-raw-materials";
    public const string ManufacturingGetFinishedGoodsRoute = "get-finished-goods";
    public const string ManufacturingGetMachineStatusesRoute = "get-machine-statuses";
}