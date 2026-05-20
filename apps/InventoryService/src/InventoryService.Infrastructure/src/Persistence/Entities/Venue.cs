namespace InventoryService.Infrastructure.Persistence.Entities;

public class Venue
{
    public Guid   Id      { get; set; }
    public string Name    { get; set; } = string.Empty;
    public string Type    { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public ICollection<Seat>  Seats  { get; set; } = [];
    public ICollection<Event> Events { get; set; } = [];
}
