namespace SharedContracts.Saga.Commands;

public record CreateBookingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CustomerId,
    string? Notes,
    IReadOnlyList<BookingItemCommand> Items,
    DateTime IssuedAt
);

public record BookingItemCommand(
    Guid ResourceId,
    string ResourceType,
    string ResourceLocation,
    DateTime StartTime,
    DateTime EndTime,
    int Quantity,
    decimal UnitPrice
);
