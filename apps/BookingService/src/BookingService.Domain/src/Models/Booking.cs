using BookingService.Domain.Entities;
using BookingService.Domain.ValueObjects;

namespace BookingService.Domain.Models;

public class Booking
{
    public Guid Id            { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid CustomerId    { get; init; }
    public Guid EventId       { get; init; }  // The specific event
    public Guid SeatId        { get; init; }  // Flattened: Exactly one seat per booking!
    public required Price Price { get; init; }   // Flattened: The price for this specific seat
    public BookingStatus Status { get; private set; }
    public BookingCode Code { get; init; } = null!;
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; private set; }

    // No more List<BookingItem> or RecalculateTotalPrice() needed!

    public static Booking Create(
        Guid        correlationId,
        Guid        customerId,
        Guid        eventId,
        Guid        seatId,
        Price       price,
        BookingCode code,
        string?     notes)
    {
        return new Booking
        {
            Id            = Guid.NewGuid(),
            CorrelationId = correlationId,
            CustomerId    = customerId,
            EventId       = eventId,
            SeatId        = seatId,
            Price         = price,
            Code          = code,
            Notes         = notes,
            Status        = BookingStatus.Pending,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow
        };
    }

    public static Booking Restore(
        Guid          id,
        Guid          correlationId,
        Guid          customerId,
        Guid          eventId,
        Guid          seatId,
        Price         price,
        BookingStatus status,
        BookingCode   code,
        string?       notes,
        DateTime      createdAt,
        DateTime      updatedAt)
    {
        return new Booking
        {
            Id            = id,
            CorrelationId = correlationId,
            CustomerId    = customerId,
            EventId       = eventId,
            SeatId        = seatId,
            Price         = price,
            Status        = status,
            Code          = code,
            Notes         = notes,
            CreatedAt     = createdAt,
            UpdatedAt     = updatedAt
        };
    }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be confirmed.");

        Status = BookingStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        Status = BookingStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}