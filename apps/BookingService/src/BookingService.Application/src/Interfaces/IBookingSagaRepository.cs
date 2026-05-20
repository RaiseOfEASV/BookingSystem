using BookingService.Domain.Models;

namespace BookingService.Application.Interfaces;

public interface IBookingSagaRepository
{
    Task<BookingSaga?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);
    Task SaveAsync(BookingSaga saga, CancellationToken cancellationToken = default);
    Task UpdateAsync(BookingSaga saga, CancellationToken cancellationToken = default);
}
