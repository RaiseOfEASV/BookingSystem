namespace InventoryService.Infrastructure.Persistence.Entities;

public class SeatInventory
{
    public Guid      Id      { get; set; }
    public Guid      EventId { get; set; }
    public Guid      SeatId  { get; set; }
    public string    Status  { get; set; } = string.Empty;
    public int       Version { get; set; } = 1;
    public Guid?     HeldBy  { get; set; }
    public DateTime? HeldAt  { get; set; }

    public Event Event { get; set; } = null!;
    public Seat  Seat  { get; set; } = null!;
}
