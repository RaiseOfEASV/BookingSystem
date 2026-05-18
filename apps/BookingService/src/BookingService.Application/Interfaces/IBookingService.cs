using BookingService.Application.Models;

namespace BookingService.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BookingDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BookingDto>> GetByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken = default);
    Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingDto?> UpdateAsync(Guid id, UpdateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
