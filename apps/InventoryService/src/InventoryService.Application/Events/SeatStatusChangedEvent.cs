namespace InventoryService.Application.Events;

public record SeatStatusChangedEvent(Guid EventId, Guid SeatId, string Status, int Version, Guid SagaId);
