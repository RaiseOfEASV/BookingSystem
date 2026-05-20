namespace InventoryService.Infrastructure.Persistence.Entities;

public class Seat
{
    public Guid   Id       { get; set; }
    public Guid   VenueId  { get; set; }
    public string Row      { get; set; } = string.Empty;
    public int    Number   { get; set; }
    public string Section  { get; set; } = string.Empty;

    public Venue                        Venue          { get; set; } = null!;
    public ICollection<SeatInventory>   SeatInventories { get; set; } = [];
}
