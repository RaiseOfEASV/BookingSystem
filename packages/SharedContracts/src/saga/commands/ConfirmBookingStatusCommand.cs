namespace SharedContracts.Saga.Commands;

public record ConfirmBookingStatusCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid BookingId,
    DateTime IssuedAt
);
