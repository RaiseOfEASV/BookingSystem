namespace SharedContracts.Saga.Compensate;

public record UpdateBookingStatusToCancelledCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid BookingId,
    Guid CustomerId,
    string Reason,
    DateTime IssuedAt
);
