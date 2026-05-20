using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.Models;

public record SeatBookingRequest(
    [Required] Guid EventId,
    [Required] string SeatId,
    [Required] Guid ReservationToken  
);
