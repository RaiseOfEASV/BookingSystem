using BookingService.Application.Interfaces;
using BookingService.Application.Models;
using BookingService.Domain.Entities;

namespace BookingService.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;

    public BookingService(IBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<BookingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _repository.GetAllAsync(cancellationToken);
        return bookings.Select(MapToDto);
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await _repository.GetByIdAsync(id, cancellationToken);
        return booking is null ? null : MapToDto(booking);
    }

    public async Task<IEnumerable<BookingDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var bookings = await _repository.GetByCustomerIdAsync(customerId, cancellationToken);
        return bookings.Select(MapToDto);
    }

    public async Task<IEnumerable<BookingDto>> GetByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        var bookings = await _repository.GetByResourceIdAsync(resourceId, cancellationToken);
        return bookings.Select(MapToDto);
    }

    public async Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var items = request.Items.Select(i => new BookingItem
        {
            Id = Guid.NewGuid(),
            ResourceId = i.ResourceId,
            ResourceType = i.ResourceType,
            ResourceLocation = i.ResourceLocation,
            StartTime = i.StartTime,
            EndTime = i.EndTime,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            SubTotal = i.UnitPrice * i.Quantity
        }).ToList();

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Status = BookingStatus.Pending,
            Notes = request.Notes,
            TotalPrice = items.Sum(i => i.SubTotal),
            Items = items,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(booking, cancellationToken);
        return MapToDto(created);
    }

    public async Task<BookingDto?> UpdateAsync(Guid id, UpdateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _repository.GetByIdAsync(id, cancellationToken);
        if (booking is null) return null;
        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed) return null;

        booking.Notes = request.Notes;
        booking.Items = request.Items.Select(i => new BookingItem
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            ResourceId = i.ResourceId,
            ResourceType = i.ResourceType,
            ResourceLocation = i.ResourceLocation,
            StartTime = i.StartTime,
            EndTime = i.EndTime,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            SubTotal = i.UnitPrice * i.Quantity
        }).ToList();
        booking.TotalPrice = booking.Items.Sum(i => i.SubTotal);
        booking.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(booking, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<BookingDto?> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await _repository.GetByIdAsync(id, cancellationToken);
        if (booking is null) return null;
        if (booking.Status == BookingStatus.Cancelled) return MapToDto(booking);

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(booking, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await _repository.GetByIdAsync(id, cancellationToken);
        if (booking is null) return false;

        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static BookingDto MapToDto(Booking b) => new(
        b.Id,
        b.CustomerId,
        b.Status,
        b.Status.ToString(),
        b.TotalPrice,
        b.Notes,
        b.Items.Select(MapItemToDto).ToList(),
        b.CreatedAt,
        b.UpdatedAt
    );

    private static BookingItemDto MapItemToDto(BookingItem i) => new(
        i.Id,
        i.ResourceId,
        i.ResourceType,
        i.ResourceType.ToString(),
        i.ResourceLocation,
        i.StartTime,
        i.EndTime,
        i.Quantity,
        i.UnitPrice,
        i.SubTotal
    );
}
