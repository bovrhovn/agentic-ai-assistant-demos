namespace AAI.Models;

public class Machine
{
    public required string MachineId { get; set; }
    public required string Status { get; set; }
    public double Temperature { get; set; }
    public double Vibration { get; set; }
    public DateTime Timestamp { get; set; }
}