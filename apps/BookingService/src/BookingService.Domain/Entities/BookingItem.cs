namespace BookingService.Domain.Entities;

public class BookingItem
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public Guid ResourceId { get; set; }
    public ResourceType ResourceType { get; set; }
    public string ResourceLocation { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}
