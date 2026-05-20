namespace SharedContracts.Saga.Commands;
public record ReserveSeatCommand(Guid EventId, Guid SeatId, Guid CorrelationId);



