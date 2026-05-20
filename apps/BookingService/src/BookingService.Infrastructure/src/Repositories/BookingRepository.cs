using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Domain.Models;
using BookingService.Domain.ValueObjects;
using BookingService.Infrastructure.Persistence;
using BookingService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var entity = ToEntity(booking);

        _context.Bookings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDomain(entity);
    }

    public async Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == booking.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Booking {booking.Id} not found.");

        entity.Status    = booking.Status.ToString();
        entity.Notes     = booking.Notes;
        entity.UpdatedAt = booking.UpdatedAt;

        await _context.SaveChangesAsync(cancellationToken);

        return ToDomain(entity);
    }

    // ── Mapping ───────────────────────────────────────────────────────────────────

    private static BookingEntity ToEntity(Booking b) => new()
    {
        Id         = b.Id,
        CustomerId = b.CustomerId,
        EventId    = b.EventId,
        SeatId     = b.SeatId,
        Amount     = b.Price.Amount,
        Currency   = b.Price.Currency,
        Status     = b.Status.ToString(),
        Code       = b.Code.Value,
        Notes      = b.Notes,
        CreatedAt  = b.CreatedAt,
        UpdatedAt  = b.UpdatedAt
    };

    private static Booking ToDomain(BookingEntity e) => Booking.Restore(
        id:         e.Id,
        customerId: e.CustomerId,
        eventId:    e.EventId,
        seatId:     e.SeatId,
        price:      new Price(e.Amount, e.Currency),
        status:     Enum.Parse<BookingStatus>(e.Status),
        code:       BookingCode.From(e.Code),
        notes:      e.Notes,
        createdAt:  e.CreatedAt,
        updatedAt:  e.UpdatedAt);
}
