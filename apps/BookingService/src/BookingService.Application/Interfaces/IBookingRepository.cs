using BookingService.Domain.Entities;

namespace BookingService.Application.Interfaces;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken = default);
    Task<int> GetBookedUnitsAsync(Guid resourceId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
    Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
