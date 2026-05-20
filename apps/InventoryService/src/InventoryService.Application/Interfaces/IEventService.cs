using InventoryService.Application.Models;

namespace InventoryService.Application.Interfaces;

public interface IEventService
{
    Task<IEnumerable<EventDto>>        GetAllAsync(CancellationToken cancellationToken = default);
    Task<EventDto?>                    GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventDto>                     CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default);
    Task<EventDto?>                    UpdateAsync(Guid id, UpdateEventRequest request, CancellationToken cancellationToken = default);
    Task<bool>                         DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AvailableSeatDto>> GetAvailableSeatsAsync(Guid eventId, CancellationToken cancellationToken = default);
}
