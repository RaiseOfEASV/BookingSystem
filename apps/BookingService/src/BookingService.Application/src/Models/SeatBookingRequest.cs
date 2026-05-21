using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.Models;

public record SeatBookingRequest(
    [Required] Guid    EventId,
    [Required] Guid    SeatId,
    [Required] Guid    ReservationToken,
    [Required] Guid    CustomerId,
    [Required] decimal Amount,
    [Required] string  Currency,
               string? Notes
);
