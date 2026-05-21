namespace BookingService.Application.Events.Definitions;

public record BookingCreatedEvent(
    Guid CorrelationId,
    Guid BookingId);
