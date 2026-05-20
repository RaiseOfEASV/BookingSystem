namespace BookingService.Infrastructure.Persistence.Entities;

public class BookingSagaEntity
{
    public Guid      Id            { get; set; }
    public Guid      EventId       { get; set; }
    public Guid    SeatId        { get; set; } 
    public Guid?     BookingId     { get; set; }
    public Guid      CorrelationId { get; set; }
    public string?   PaymentId     { get; set; }
    public string    Status        { get; set; } = null!;
    public string?   FailureReason { get; set; }
    public DateTime  CreatedAt     { get; set; }
    public DateTime  UpdatedAt     { get; set; }
}
