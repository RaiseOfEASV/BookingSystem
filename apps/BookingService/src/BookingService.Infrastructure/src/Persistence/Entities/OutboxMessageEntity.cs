namespace BookingService.Infrastructure.Persistence.Entities;

public class OutboxMessageEntity
{
    public Guid      Id          { get; set; }
    public DateTime  OccurredOn  { get; set; }
    public string    Type        { get; set; } = null!;
    public string    Content     { get; set; } = null!;
    public DateTime? ProcessedOn { get; set; }
}