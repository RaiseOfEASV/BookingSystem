namespace BookingService.Application.Models;

public record SeatClaimResult(
    Guid EventId,
    string SeatId,
    Guid ReservationToken,
    bool Claimed
);

public record SeatAvailabilityResult(
    Guid EventId,
    string SeatId,
    bool Available
);
