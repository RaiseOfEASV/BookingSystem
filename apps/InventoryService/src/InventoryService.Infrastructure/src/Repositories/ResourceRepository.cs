using System.Data;
using Exceptions;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Models;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly InventoryDbContext _context;

    public ResourceRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SeatInventory>> GetAvailableSeatsForEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
        => await _context.SeatInventories
            .Where(si => si.EventId == eventId && si.Status == SeatStatusMapper.ToString(SeatStatus.Available))
            .Include(si => si.Seat)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    async Task<SeatInventoryDto?> IResourceRepository.GetSeatInventoryAsync(Guid eventId, Guid seatId, CancellationToken cancellationToken)
    {
        var entity = await _context.SeatInventories
            .Where(si => si.EventId == eventId && si.SeatId == seatId && si.Status == SeatStatusMapper.ToString(SeatStatus.Available))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null) return null;

        return new SeatInventoryDto
        {
            EventId = entity.EventId,
            SeatId  = entity.SeatId,
            Status  = entity.Status,
            Version = entity.Version,
            HeldBy  = entity.HeldBy,
            HeldAt  = entity.HeldAt
        };
    }

    public async Task UpdateSeatStatusAsync(SeatInventoryDto seatInventory, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, 
            cancellationToken);

        try
        {
            var updated = await _context.SeatInventories
                .Where(si => si.SeatId  == seatInventory.SeatId  &&
                             si.EventId == seatInventory.EventId &&
                             si.Version == seatInventory.Version-1)
                .ExecuteUpdateAsync(si => si
                        .SetProperty(x => x.Status,  seatInventory.Status)
                        .SetProperty(x => x.Version, seatInventory.Version)
                        .SetProperty(x => x.HeldBy,  seatInventory.HeldBy)
                        .SetProperty(x => x.HeldAt,  seatInventory.HeldAt),
                    cancellationToken);

            if (updated == 0)
                throw new SeatReservedException(
                    $"Seat '{seatInventory.SeatId}' was already reserved by another process.");

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw; 
        }
    }
    Task<IEnumerable<SeatInventoryDto>> IResourceRepository.GetAvailableSeatsForEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<SeatInventory?> GetSeatInventoryAsync(
        Guid eventId,
        Guid seatId,
        CancellationToken cancellationToken = default)
        => await _context.SeatInventories
            .FirstOrDefaultAsync(si => si.EventId == eventId && si.SeatId == seatId, cancellationToken);

    public async Task UpdateSeatStatusAsync(
        SeatInventory seatInventory,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _context.SeatInventories.Update(seatInventory);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
