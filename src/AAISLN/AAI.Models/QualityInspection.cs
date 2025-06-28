namespace AAI.Models;

public class QualityInspection
{
    public required string InspectionId { get; set; }
    public required string Product { get; set; }
    public required string Result { get; set; }
    public required string Inspector { get; set; }
    public DateTime Timestamp { get; set; }
}