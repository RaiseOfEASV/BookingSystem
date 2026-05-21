using BookingService.Application.Models;
using BookingService.Domain.Models;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Booking?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
    Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task<BookingSaga?> GetSagaByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
    Task SaveAsync(BookingSaga saga, OutboxMessageDto messageDto , CancellationToken cancellationToken = default);
    Task UpdateAsync(BookingSaga saga, CancellationToken cancellationToken = default);
}
