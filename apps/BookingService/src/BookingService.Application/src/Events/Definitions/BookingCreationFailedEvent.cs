namespace BookingService.Application.Events.Definitions;

public record BookingCreationFailedEvent(
    Guid CorrelationId,
    Guid BookingId,
    string Reason);