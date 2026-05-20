using System.ComponentModel.DataAnnotations;
using BookingService.Domain.ValueObjects;

namespace BookingService.Application.Models;

public record CreateBookingRequest(
    [Required] Guid    CustomerId,
    [Required] Guid    EventId,
    [Required] Guid    SeatId,
    [Required] Price Amount,
    [Required] string  Currency,
    string?            Notes
);
