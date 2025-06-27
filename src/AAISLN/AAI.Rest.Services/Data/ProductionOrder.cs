namespace AAI.Rest.Services.Data;

public class ProductionOrder
{
    public required string OrderId { get; set; }
    public required string Product { get; set; }
    public int Quantity { get; set; }
    public required string Status { get; set; }
    public DateTime DueDate { get; set; }
}