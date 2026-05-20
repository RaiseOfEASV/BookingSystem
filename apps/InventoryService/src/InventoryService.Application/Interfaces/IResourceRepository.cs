using InventoryService.Domain.Models;

namespace InventoryService.Application.Interfaces;

public interface IResourceRepository
{
    Task<IEnumerable<SeatInventoryDto>> GetAvailableSeatsForEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<SeatInventoryDto?> GetSeatInventoryAsync(Guid eventId, Guid seatId, CancellationToken cancellationToken = default);
    Task UpdateSeatStatusAsync(SeatInventoryDto seatInventory, CancellationToken cancellationToken = default);
}
