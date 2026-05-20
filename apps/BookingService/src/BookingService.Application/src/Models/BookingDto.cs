namespace BookingService.Application.Models;

public record BookingDto(
    Guid     Id,
    Guid     CustomerId,
    Guid     EventId,
    Guid     SeatId,
    string   Code,
    string   Status,
    decimal  Amount,
    string   Currency,
    string?  Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
