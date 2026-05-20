using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly InventoryDbContext _context;

    public EventRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Events
            .AsNoTracking()
            .Select(e => new EventDto(e.Id, e.VenueId, e.EventName, e.StartDate, e.EndDate))
            .ToListAsync(cancellationToken);

    public async Task<EventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<EventDto> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Event
        {
            Id        = Guid.NewGuid(),
            VenueId   = request.VenueId,
            EventName = request.EventName,
            StartDate = request.StartDate,
            EndDate   = request.EndDate
        };

        _context.Events.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<EventDto?> UpdateAsync(Guid id, UpdateEventRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null) return null;

        entity.EventName = request.EventName;
        entity.StartDate = request.StartDate;
        entity.EndDate   = request.EndDate;

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _context.Events
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }

    public async Task<IEnumerable<AvailableSeatDto>> GetAvailableSeatsAsync(Guid eventId, CancellationToken cancellationToken = default)
        => await _context.SeatInventories
            .Where(si => si.EventId == eventId && si.Status == SeatStatusMapper.ToString(SeatStatus.Available))
            .Include(si => si.Seat)
            .AsNoTracking()
            .Select(si => new AvailableSeatDto(
                si.SeatId,
                si.EventId,
                si.Seat.Row,
                si.Seat.Number,
                si.Seat.Section,
                si.Status))
            .ToListAsync(cancellationToken);

    private static EventDto ToDto(Event e) => new(e.Id, e.VenueId, e.EventName, e.StartDate, e.EndDate);
}
