namespace AAI.Rest.Services.Data;

using Bogus;
using System;
using System.Collections.Generic;

public static class FakeDataGenerator
{
    private static readonly string[] Statuses = { "Scheduled", "In Progress", "Completed", "Delayed" };
    private static readonly string[] MachineStatuses = { "Running", "Idle", "Maintenance", "Error" };
    private static readonly string[] Units = { "kg", "pcs", "liters" };
    private static readonly string[] Shifts = { "Morning", "Evening", "Night" };

    public class ProductionOrder
    {
        public string OrderId { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class Machine
    {
        public string MachineId { get; set; }
        public string Status { get; set; }
        public double Temperature { get; set; }
        public double Vibration { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class InventoryItem
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public string Location { get; set; }
    }

    public class QualityInspection
    {
        public string InspectionId { get; set; }
        public string Product { get; set; }
        public string Result { get; set; }
        public string Inspector { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MaintenanceLog
    {
        public string LogId { get; set; }
        public string MachineId { get; set; }
        public string Description { get; set; }
        public string PerformedBy { get; set; }
        public DateTime Date { get; set; }
    }

    public class WorkforceShift
    {
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Shift { get; set; }
        public string AssignedMachine { get; set; }
    }
    
    public static List<string> GetMachineStatuses() => [..Statuses];

    public static List<ProductionOrder> GenerateProductionOrders(int count = 10)
    {
        var faker = new Faker<ProductionOrder>()
            .RuleFor(o => o.OrderId, f => $"PO-{f.Random.Number(1000, 9999)}")
            .RuleFor(o => o.Product, f => f.Commerce.ProductName())
            .RuleFor(o => o.Quantity, f => f.Random.Int(50, 500))
            .RuleFor(o => o.Status, f => f.PickRandom(Statuses))
            .RuleFor(o => o.DueDate, f => f.Date.Future(1));

        return faker.Generate(count);
    }

    public static List<Machine> GenerateMachines(int count = 5)
    {
        var faker = new Faker<Machine>()
            .RuleFor(m => m.MachineId, f => $"M-{f.Random.Number(100, 999)}")
            .RuleFor(m => m.Status, f => f.PickRandom(MachineStatuses))
            .RuleFor(m => m.Temperature, f => f.Random.Double(60, 100))
            .RuleFor(m => m.Vibration, f => Math.Round(f.Random.Double(0.5, 10.0), 2))
            .RuleFor(m => m.Timestamp, f => DateTime.UtcNow);

        return faker.Generate(count);
    }

    public static List<InventoryItem> GenerateInventoryItems(int count = 10, bool isRawMaterial = true)
    {
        var faker = new Faker<InventoryItem>()
            .RuleFor(i => i.ItemId, f => isRawMaterial ? $"RM-{f.Random.Number(100, 999)}" : $"FG-{f.Random.Number(100, 999)}")
            .RuleFor(i => i.Name, f => isRawMaterial ? f.Commerce.ProductMaterial() : f.Commerce.ProductName())
            .RuleFor(i => i.Quantity, f => f.Random.Int(100, 1000))
            .RuleFor(i => i.Unit, f => f.PickRandom(Units))
            .RuleFor(i => i.Location, f => $"Warehouse {f.Random.AlphaNumeric(1).ToUpper()}");

        return faker.Generate(count);
    }

    public static List<QualityInspection> GenerateQualityInspections(int count = 5)
    {
        var faker = new Faker<QualityInspection>()
            .RuleFor(q => q.InspectionId, f => $"INSP-{f.Random.Number(1000, 9999)}")
            .RuleFor(q => q.Product, f => f.Commerce.ProductName())
            .RuleFor(q => q.Result, f => f.Random.Bool(0.8f) ? "Pass" : "Fail")
            .RuleFor(q => q.Inspector, f => f.Name.FullName())
            .RuleFor(q => q.Timestamp, f => f.Date.Recent(5));

        return faker.Generate(count);
    }

    public static List<MaintenanceLog> GenerateMaintenanceLogs(int count = 5)
    {
        var faker = new Faker<MaintenanceLog>()
            .RuleFor(m => m.LogId, f => $"LOG-{f.Random.Number(1000, 9999)}")
            .RuleFor(m => m.MachineId, f => $"M-{f.Random.Number(100, 999)}")
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.PerformedBy, f => f.Name.FullName())
            .RuleFor(m => m.Date, f => f.Date.Past(1));

        return faker.Generate(count);
    }

    public static List<WorkforceShift> GenerateWorkforceShifts(int count = 5)
    {
        var faker = new Faker<WorkforceShift>()
            .RuleFor(w => w.EmployeeId, f => $"E-{f.Random.Number(1000, 9999)}")
            .RuleFor(w => w.Name, f => f.Name.FullName())
            .RuleFor(w => w.Shift, f => f.PickRandom(Shifts))
            .RuleFor(w => w.AssignedMachine, f => $"M-{f.Random.Number(100, 999)}");

        return faker.Generate(count);
    }

    public static Machine GenerateMachine(string machineId)
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
