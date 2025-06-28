namespace AAI.Models;

public class MaintenanceLog
{
    public required string LogId { get; set; }
    public required string MachineId { get; set; }
    public required string Description { get; set; }
    public required string PerformedBy { get; set; }
    public DateTime Date { get; set; }
}