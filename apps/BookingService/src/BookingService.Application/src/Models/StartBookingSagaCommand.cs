namespace BookingService.Application.Models;

public record StartBookingSagaCommand(
    Guid    CorrelationId,
    Guid    EventId,
    Guid    SeatId,
    Guid    CustomerId,
    decimal Amount,
    string  Currency,
    string? Notes
);
