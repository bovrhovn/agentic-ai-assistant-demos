namespace AAI.Rest.Services.Data;

public class InventoryItem
{
    public required string ItemId { get; set; }
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public required string Unit { get; set; }
    public required string Location { get; set; }
}