using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;

namespace InventoryService.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<EventDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => _repository.GetAllAsync(cancellationToken);

    public Task<EventDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetByIdAsync(id, cancellationToken);

    public Task<EventDto> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default)
        => _repository.CreateAsync(request, cancellationToken);

    public Task<EventDto?> UpdateAsync(Guid id, UpdateEventRequest request, CancellationToken cancellationToken = default)
        => _repository.UpdateAsync(id, request, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);

    public Task<IEnumerable<AvailableSeatDto>> GetAvailableSeatsAsync(Guid eventId, CancellationToken cancellationToken = default)
        => _repository.GetAvailableSeatsAsync(eventId, cancellationToken);
}
