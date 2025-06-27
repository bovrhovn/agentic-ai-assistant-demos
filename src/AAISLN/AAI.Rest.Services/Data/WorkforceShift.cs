namespace AAI.Rest.Services.Data;

public class WorkforceShift
{
    public required string EmployeeId { get; set; }
    public required string Name { get; set; }
    public required string Shift { get; set; }
    public required string AssignedMachine { get; set; }
}