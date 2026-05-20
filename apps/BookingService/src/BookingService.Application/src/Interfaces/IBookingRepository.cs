using BookingService.Domain.Models;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
}
