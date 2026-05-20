namespace InventoryService.Infrastructure.Persistence.Entities;

public class MessageEntity
{
    public Guid      Id            { get; set; }

    // ReserveSeatCommand payload (flattened)
    public Guid      EventId       { get; set; }
    public Guid      SeatId        { get; set; }
    public Guid      CorrelationId { get; set; }

    public string    Status        { get; set; } = string.Empty;
    public int       RetryCount    { get; set; } = 0;
    public string?   LastError     { get; set; }
    public DateTime  ReceivedAt    { get; set; }
    public DateTime  UpdatedAt     { get; set; }
    public DateTime? ProcessedAt   { get; set; }
}