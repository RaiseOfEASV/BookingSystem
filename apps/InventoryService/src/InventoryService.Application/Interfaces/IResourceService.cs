using InventoryService.Application.Models;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Interfaces;

public interface IResourceService
{
    Task<IEnumerable<ResourceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceDto>> GetByTypeAsync(ResourceType type, CancellationToken cancellationToken = default);
    Task<ResourceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ResourceDto> CreateAsync(CreateResourceRequest request, CancellationToken cancellationToken = default);
    Task<ResourceDto?> UpdateAsync(Guid id, UpdateResourceRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AvailabilityDto?> CheckAvailabilityAsync(Guid id, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
}
