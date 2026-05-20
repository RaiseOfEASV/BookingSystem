namespace BookingService.Infrastructure.Persistence.Entities;

public class BookingEntity
{
    public Guid     Id         { get; set; }
    public Guid     CustomerId { get; set; }
    public Guid     EventId    { get; set; }
    public Guid     SeatId     { get; set; }
    public decimal  Amount     { get; set; }
    public string   Currency   { get; set; } = null!;
    public string   Status     { get; set; } = null!;
    public string   Code       { get; set; } = null!;
    public string?  Notes      { get; set; }
    public DateTime CreatedAt  { get; set; }
    public DateTime UpdatedAt  { get; set; }
}
