using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IResourceRepository
{
    Task<IEnumerable<Resource>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Resource>> GetByTypeAsync(ResourceType type, CancellationToken cancellationToken = default);
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Resource> CreateAsync(Resource resource, CancellationToken cancellationToken = default);
    Task<Resource> UpdateAsync(Resource resource, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetAvailableUnitsAsync(Guid resourceId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
}
