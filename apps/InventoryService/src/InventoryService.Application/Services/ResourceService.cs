using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _repository;

    public ResourceService(IResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ResourceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var resources = await _repository.GetAllAsync(cancellationToken);
        return resources.Select(MapToDto);
    }

    public async Task<IEnumerable<ResourceDto>> GetByTypeAsync(ResourceType type, CancellationToken cancellationToken = default)
    {
        var resources = await _repository.GetByTypeAsync(type, cancellationToken);
        return resources.Select(MapToDto);
    }

    public async Task<ResourceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resource = await _repository.GetByIdAsync(id, cancellationToken);
        return resource is null ? null : MapToDto(resource);
    }

    public async Task<ResourceDto> CreateAsync(CreateResourceRequest request, CancellationToken cancellationToken = default)
    {
        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            Location = request.Location,
            Capacity = request.Capacity,
            PricePerUnit = request.PricePerUnit,
            Currency = request.Currency,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(resource, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ResourceDto?> UpdateAsync(Guid id, UpdateResourceRequest request, CancellationToken cancellationToken = default)
    {
        var resource = await _repository.GetByIdAsync(id, cancellationToken);
        if (resource is null) return null;

        resource.Name = request.Name;
        resource.Description = request.Description;
        resource.Location = request.Location;
        resource.Capacity = request.Capacity;
        resource.PricePerUnit = request.PricePerUnit;
        resource.Currency = request.Currency;
        resource.IsActive = request.IsActive;
        resource.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(resource, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var resource = await _repository.GetByIdAsync(id, cancellationToken);
        if (resource is null) return false;

        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<AvailabilityDto?> CheckAvailabilityAsync(Guid id, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        var resource = await _repository.GetByIdAsync(id, cancellationToken);
        if (resource is null) return null;

        var availableUnits = await _repository.GetAvailableUnitsAsync(id, startTime, endTime, cancellationToken);
        var bookedUnits = resource.Capacity - availableUnits;

        return new AvailabilityDto(
            resource.Id,
            resource.Name,
            startTime,
            endTime,
            resource.Capacity,
            bookedUnits,
            availableUnits,
            availableUnits > 0
        );
    }

    private static ResourceDto MapToDto(Resource r) => new(
        r.Id,
        r.Name,
        r.Type,
        r.Type.ToString(),
        r.Description,
        r.Location,
        r.Capacity,
        r.PricePerUnit,
        r.Currency,
        r.IsActive,
        r.CreatedAt,
        r.UpdatedAt
    );
}
