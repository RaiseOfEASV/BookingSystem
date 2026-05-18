namespace SharedContracts.Saga.Compensate;

public record ReleaseSeatCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid BookingId,
    Guid ResourceId,
    string ResourceType,
    string ResourceLocation,
    DateTime StartTime,
    DateTime EndTime,
    int Quantity,
    DateTime IssuedAt
);
