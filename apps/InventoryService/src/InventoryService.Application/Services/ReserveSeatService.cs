using Exceptions;
using InventoryService.Application.Interfaces;
using MessageClient.Interfaces;
using SharedContracts.Saga.Commands;
using SharedContracts.Saga.Events;

namespace InventoryService.Application.Services;

public sealed class ReserveSeatService : IReserveSeatService
{
    private readonly IResourceRepository _repository;
    private readonly IMessageClient _messageClient;


    public ReserveSeatService(IResourceRepository repository, IMessageClient messageClient)
    {
        _repository = repository;
        _messageClient = messageClient;
    }

    public async Task HandleReserveSeat(ReserveSeatCommand command, CancellationToken cancellationToken)
    {
        var seatInventory = await _repository.GetSeatInventoryAsync(
            command.EventId, command.SeatId, cancellationToken);

        if (seatInventory is null)
        {
            await _messageClient.PublishAsync(new SeatReservationFailedEvent(
                command.EventId,
                command.SeatId,
                command.CorrelationId,
                $"No inventory record found for event '{command.EventId}' and seat '{command.SeatId}'.",
            SeatReservationFailureCode.SeatNotFound)
            
            );
            return;
        }

        if (seatInventory.Status == SeatStatusMapper.ToString(SeatStatus.Held) &&
            seatInventory.HeldBy == command.CorrelationId)
        {
            await _messageClient.PublishAsync(new SeatReservedEvent(
                command.SeatId, command.CorrelationId));
            return;
        }

        if (seatInventory.Status != SeatStatusMapper.ToString(SeatStatus.Available))
        {
            await _messageClient.PublishAsync(new SeatReservationFailedEvent(
                command.EventId,
                command.SeatId,
                command.CorrelationId,
                $"Seat '{command.SeatId}' is not available. Current status: '{seatInventory.Status}'.",
                SeatReservationFailureCode.SeatUnavailable
                )
              );
            return;
        }

        seatInventory.Status = SeatStatusMapper.ToString(SeatStatus.Held);
        seatInventory.Version += 1;
        seatInventory.HeldBy = command.CorrelationId;
        seatInventory.HeldAt = DateTime.UtcNow;

        try
        {
            await _repository.UpdateSeatStatusAsync(seatInventory, cancellationToken);

            await _messageClient.PublishAsync(new SeatReservedEvent(
                command.SeatId, command.CorrelationId));
        }
        catch (SeatReservedException ex)
        {
            // race condition , optimistic lock another process reserved it between our read and write
            await _messageClient.PublishAsync(new SeatReservationFailedEvent(
                command.EventId,
                command.SeatId, 
                command.CorrelationId,
                $"Seat '{command.SeatId}' was taken by another process: {ex.Message}",
                SeatReservationFailureCode.SeatUnavailable
                ));
        }
        // all other exceptions bubble up to worker, mark the message to fail and update retry 
    }

    public async Task HandleMessageProcessingFailed(SeatReservationFailedEvent message,
        CancellationToken cancellationToken)
    {
        await _messageClient.PublishAsync(message);
    }
}

