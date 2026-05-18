using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Bookings
            .Include(b => b.Items)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Bookings
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await _context.Bookings
            .Include(b => b.Items)
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Booking>> GetByResourceIdAsync(Guid resourceId, CancellationToken cancellationToken = default)
        => await _context.Bookings
            .Include(b => b.Items)
            .Where(b => b.Items.Any(i => i.ResourceId == resourceId))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<int> GetBookedUnitsAsync(Guid resourceId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
        => await _context.BookingItems
            .Where(i => i.ResourceId == resourceId
                && i.Booking.Status != BookingStatus.Cancelled
                && i.StartTime < endTime
                && i.EndTime > startTime)
            .SumAsync(i => i.Quantity, cancellationToken);

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);
        return booking;
    }

    public async Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        // Remove old items and let EF replace them with the new set
        var existingItems = await _context.BookingItems
            .Where(i => i.BookingId == booking.Id)
            .ToListAsync(cancellationToken);

        _context.BookingItems.RemoveRange(existingItems);
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync(cancellationToken);
        return booking;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FindAsync([id], cancellationToken);
        if (booking is not null)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
