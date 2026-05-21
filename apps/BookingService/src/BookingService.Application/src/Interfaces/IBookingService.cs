using BookingService.Application.Events.Definitions;
using BookingService.Application.Models;

namespace BookingService.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResult> CreateAsync(
        CreateBookingRequest request, 
        CancellationToken cancellationToken = default);
    Task<BookingDto> ConfirmAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<BookingDto> CancelAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
