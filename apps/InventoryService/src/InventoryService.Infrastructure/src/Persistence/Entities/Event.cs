namespace InventoryService.Infrastructure.Persistence.Entities;

public class Event
{
    public Guid     Id        { get; set; }
    public Guid     VenueId   { get; set; }
    public string   EventName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate   { get; set; }

    public Venue                      Venue           { get; set; } = null!;
    public ICollection<SeatInventory> SeatInventories { get; set; } = [];
}
