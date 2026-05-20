using SharedContracts.Saga.Commands;
using SharedContracts.Saga.Events;

namespace InventoryService.Application.Interfaces;

public interface IReserveSeatService
{
    Task HandleReserveSeat(ReserveSeatCommand command, CancellationToken cancellationToken);
    Task HandleMessageProcessingFailed(SeatReservationFailedEvent seatReservationFailedEvent, CancellationToken cancellationToken);
}