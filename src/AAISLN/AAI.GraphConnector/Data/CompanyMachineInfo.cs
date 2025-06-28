namespace AAI.GraphConnector.Data;

public class CompanyMachineInfo
{
    public required string MachineId { get; set; }
    public required string Title { get; set; }
    public DateTime Activated { get; set; } = DateTime.Now;
    public long RestartCount { get; set; } = 0;
    public string Message { get; set; } = string.Empty;
}