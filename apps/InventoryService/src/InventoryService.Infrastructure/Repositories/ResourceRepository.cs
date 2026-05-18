using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly InventoryDbContext _context;

    public ResourceRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Resource>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Resources.Where(r => r.IsActive).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Resource>> GetByTypeAsync(ResourceType type, CancellationToken cancellationToken = default)
        => await _context.Resources.Where(r => r.Type == type && r.IsActive).ToListAsync(cancellationToken);

    public async Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Resources.FindAsync([id], cancellationToken);

    public async Task<Resource> CreateAsync(Resource resource, CancellationToken cancellationToken = default)
    {
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync(cancellationToken);
        return resource;
    }

    public async Task<Resource> UpdateAsync(Resource resource, CancellationToken cancellationToken = default)
    {
        _context.Resources.Update(resource);
        await _context.SaveChangesAsync(cancellationToken);
        return resource;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resource = await _context.Resources.FindAsync([id], cancellationToken);
        if (resource is not null)
        {
            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Returns available units by checking an external bookings count.
    // Since InventoryService does not own bookings, this defaults to full capacity.
    // In a real system, this would call the BookingService or use an event-sourced read model.
    public Task<int> GetAvailableUnitsAsync(Guid resourceId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        return _context.Resources
            .Where(r => r.Id == resourceId)
            .Select(r => r.Capacity)
            .FirstOrDefaultAsync(cancellationToken)!;
    }
}
