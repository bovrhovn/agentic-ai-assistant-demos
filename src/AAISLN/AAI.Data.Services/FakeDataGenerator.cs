using AAI.Models;
using Bogus;
using Microsoft.Extensions.Caching.Memory;

namespace AAI.Data.Services;

public class FakeDataGenerator(IMemoryCache memoryCache)
{
    private static readonly string[] Statuses = ["Scheduled", "In Progress", "Completed", "Delayed"];
    private static readonly string[] MachineStatuses = ["Running", "Idle", "Maintenance", "Error"];
    private static readonly string[] Units = ["kg", "pcs", "liters"];
    private static readonly string[] Shifts = ["Morning", "Evening", "Night"];

    private const string productionOrderCacheKey = "ProductionOrders";
    private const string machineCacheKey = "Machines";
    private const string inventoryCacheKey = "InventoryItems";
    private const string qualityCacheKey = "QualityInspections";
    private const string maintainceLogCacheKey = "MaintainanceLogs";
    private const string workshiftsLogCacheKey = "WorkforceShifts";
    
    public static List<string> GetMachineStatuses() => [..Statuses];
    
    private List<T> GetFromMemoryCache<T>(string name) => 
        (memoryCache.TryGetValue(name, out List<T>? data) ? data : []) ?? new List<T>();
    
    public List<ProductionOrder> GenerateProductionOrders(int count = 10)
    {
        var list = GetFromMemoryCache<ProductionOrder>(productionOrderCacheKey);
        if (list.Count != 0) return list;
        
        var faker = new Faker<ProductionOrder>()
            .RuleFor(o => o.OrderId, f => $"PO-{f.Random.Number(1000, 9999)}")
            .RuleFor(o => o.Product, f => f.Commerce.ProductName())
            .RuleFor(o => o.Quantity, f => f.Random.Int(50, 500))
            .RuleFor(o => o.Status, f => f.PickRandom(Statuses))
            .RuleFor(o => o.DueDate, f => f.Date.Future(1));

        var productionOrder = faker.Generate(count);
        memoryCache.Set(productionOrderCacheKey, productionOrder, TimeSpan.FromHours(1));
        return productionOrder;
    }

    public List<Machine> GenerateMachines(int count = 5)
    {
        var list = GetFromMemoryCache<Machine>(machineCacheKey);
        if (list.Count != 0) return list;
        var faker = new Faker<Machine>()
            .RuleFor(m => m.MachineId, f => $"M-{f.Random.Number(100, 999)}")
            .RuleFor(m => m.Status, f => f.PickRandom(MachineStatuses))
            .RuleFor(m => m.Temperature, f => f.Random.Double(60, 100))
            .RuleFor(m => m.Vibration, f => Math.Round(f.Random.Double(0.5, 10.0), 2))
            .RuleFor(m => m.Timestamp, f => DateTime.UtcNow);

        var machines = faker.Generate(count);
        memoryCache.Set(machineCacheKey, machines, TimeSpan.FromHours(1));
        return machines;
    }

    public List<InventoryItem> GenerateInventoryItems(int count = 10, bool isRawMaterial = true)
    {
        var list = GetFromMemoryCache<InventoryItem>(inventoryCacheKey);
        if (list.Count != 0) return list;
        var faker = new Faker<InventoryItem>()
            .RuleFor(i => i.ItemId, f => isRawMaterial ? $"RM-{f.Random.Number(100, 999)}" : $"FG-{f.Random.Number(100, 999)}")
            .RuleFor(i => i.Name, f => isRawMaterial ? f.Commerce.ProductMaterial() : f.Commerce.ProductName())
            .RuleFor(i => i.Quantity, f => f.Random.Int(100, 1000))
            .RuleFor(i => i.Unit, f => f.PickRandom(Units))
            .RuleFor(i => i.Location, f => $"Warehouse {f.Random.AlphaNumeric(1).ToUpper()}");

        var inventoryItems = faker.Generate(count);
        memoryCache.Set(inventoryCacheKey, inventoryItems, TimeSpan.FromHours(1));
        return inventoryItems;
    }

    public List<QualityInspection> GenerateQualityInspections(int count = 5)
    {
        var list = GetFromMemoryCache<QualityInspection>(qualityCacheKey);
        if (list.Count != 0) return list;
        var faker = new Faker<QualityInspection>()
            .RuleFor(q => q.InspectionId, f => $"INSP-{f.Random.Number(1000, 9999)}")
            .RuleFor(q => q.Product, f => f.Commerce.ProductName())
            .RuleFor(q => q.Result, f => f.Random.Bool(0.8f) ? "Pass" : "Fail")
            .RuleFor(q => q.Inspector, f => f.Name.FullName())
            .RuleFor(q => q.Timestamp, f => f.Date.Recent(5));

        var qualityInspections = faker.Generate(count);
        memoryCache.Set(qualityCacheKey, qualityInspections, TimeSpan.FromHours(1));
        return qualityInspections;
    }

    public List<MaintenanceLog> GenerateMaintenanceLogs(int count = 5)
    {
        var list = GetFromMemoryCache<MaintenanceLog>(maintainceLogCacheKey);
        if (list.Count != 0) return list;
        var faker = new Faker<MaintenanceLog>()
            .RuleFor(m => m.LogId, f => $"LOG-{f.Random.Number(1000, 9999)}")
            .RuleFor(m => m.MachineId, f => $"M-{f.Random.Number(100, 999)}")
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.PerformedBy, f => f.Name.FullName())
            .RuleFor(m => m.Date, f => f.Date.Past(1));

        var maintainanceLogs = faker.Generate(count);
        memoryCache.Set(qualityCacheKey, maintainanceLogs, TimeSpan.FromHours(1));
        return maintainanceLogs;
    }

    public List<WorkforceShift> GenerateWorkforceShifts(int count = 5)
    {
        var list = GetFromMemoryCache<WorkforceShift>(workshiftsLogCacheKey);
        if (list.Count != 0) return list;
        
        var faker = new Faker<WorkforceShift>()
            .RuleFor(w => w.EmployeeId, f => $"E-{f.Random.Number(1000, 9999)}")
            .RuleFor(w => w.Name, f => f.Name.FullName())
            .RuleFor(w => w.Shift, f => f.PickRandom(Shifts))
            .RuleFor(w => w.AssignedMachine, f => $"M-{f.Random.Number(100, 999)}");

        var workforceShifts = faker.Generate(count);
        memoryCache.Set(workshiftsLogCacheKey, workforceShifts, TimeSpan.FromHours(1));
        return workforceShifts;
    }

    public Machine GenerateMachine(string machineId)
    {
        var faker = new Faker<Machine>()
            .RuleFor(m => m.MachineId, f => $"M-{f.Random.Number(100, 999)}")
            .RuleFor(m => m.Status, f => f.PickRandom(MachineStatuses))
            .RuleFor(m => m.Temperature, f => f.Random.Double(60, 100))
            .RuleFor(m => m.Vibration, f => Math.Round(f.Random.Double(0.5, 10.0), 2))
            .RuleFor(m => m.Timestamp, f => DateTime.UtcNow);
        var machine = faker.Generate();
        machine.MachineId = machineId; // Set the specific machine ID
        return machine;
    }
}