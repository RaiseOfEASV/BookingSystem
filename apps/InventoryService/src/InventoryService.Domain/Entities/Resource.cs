namespace InventoryService.Domain.Entities;

public class Resource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerUnit { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
