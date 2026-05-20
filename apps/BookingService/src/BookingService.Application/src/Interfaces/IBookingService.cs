using BookingService.Application.Models;

namespace BookingService.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingDto> ConfirmAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<BookingDto> CancelAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
