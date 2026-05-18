namespace SharedContracts.Saga.Commands;

public record InitiatePaymentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid BookingId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTime IssuedAt
);
