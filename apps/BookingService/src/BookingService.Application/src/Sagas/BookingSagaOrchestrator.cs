using System.Text.Json;
using BookingService.Application.Events.Definitions;
using BookingService.Application.Interfaces;
using BookingService.Application.Models;
using BookingService.Domain.Models;
using MessageClient.Interfaces;
using MessageClient.Types;
using SharedContracts.Saga.Commands;
using SharedContracts.Saga.Compensate;
using SharedContracts.Saga.Events;

namespace BookingService.Application.Sagas;

public class BookingSagaOrchestrator
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingService    _bookingService;
    private readonly IMessageClient     _messageClient;

    public BookingSagaOrchestrator(
        IBookingRepository bookingRepository,
        IBookingService    bookingService,
        IMessageClient     messageClient)
    {
        _bookingRepository = bookingRepository;
        _bookingService    = bookingService;
        _messageClient     = messageClient;
    }

    // ── STEP 1: Trigger ───────────────────────────────────────────────────────────
    // Entry point — called by the controller after the Redis claim succeeds.
    public async Task HandleAsync(StartBookingSagaCommand command, CancellationToken ct = default)
    {
        var saga = BookingSaga.Create(
            eventId:       command.EventId,
            seatId:        command.SeatId,
            customerId:    command.CustomerId,
            amount:        command.Amount,
            currency:      command.Currency,
            notes:         command.Notes,
            correlationId: command.CorrelationId);

        var reserveSeatCommand = new ReserveSeatCommand(command.EventId, command.SeatId, command.CorrelationId);

        var message = new OutboxMessageDto(
            Id:          Guid.NewGuid(),
            OccurredOn:  DateTime.UtcNow,
            Type:        typeof(ReserveSeatCommand).FullName!,
            Content:     JsonSerializer.Serialize(reserveSeatCommand),
            ProcessedOn: null);

        await _bookingRepository.SaveAsync(saga, message, ct);
    }

    // ── STEP 2a: Seat reserved ────────────────────────────────────────────────────
    // Received from InventoryService after it locks the seat in its own DB.
    public async Task HandleAsync(SeatReservedEvent @event, CancellationToken ct = default)
    {
        var saga = await _bookingRepository.GetSagaByCorrelationIdAsync(@event.CorrelationId, ct);
        if (saga is null) return;

        saga.MarkSeatReserved();
        saga.MarkCreatingBooking();
        await _bookingRepository.UpdateAsync(saga, ct);

        var result = await _bookingService.CreateAsync(
            new CreateBookingRequest(
                CorrelationId: saga.CorrelationId,
                CustomerId:    saga.CustomerId,
                EventId:       saga.EventId,
                SeatId:        saga.SeatId,
                Amount:        new Domain.ValueObjects.Price(saga.Amount, saga.Currency),
                Currency:      saga.Currency,
                Notes:         saga.Notes),
            ct);

        if (result.IsFailure)
        {
            saga.Fail(result.Failure.Reason);
            saga.Compensate();
            await _bookingRepository.UpdateAsync(saga, ct);
            return;
        }

        saga.MarkBookingCreated(result.Success.BookingId);
        saga.MarkPaymentPending();
        await _bookingRepository.UpdateAsync(saga, ct);

        await _messageClient.PublishAsync(
            new InitiatePaymentCommand(
                CommandId:     Guid.NewGuid(),
                CorrelationId: saga.CorrelationId,
                BookingId:     result.Success.BookingId,
                CustomerId:    saga.CustomerId,
                Amount:        saga.Amount,
                Currency:      saga.Currency,
                IssuedAt:      DateTime.UtcNow));
    }

    // ── STEP 2b: Seat reservation failed (compensate) ─────────────────────────────
    public async Task HandleAsync(SeatReservationFailedEvent @event, CancellationToken ct = default)
    {
        var saga = await _bookingRepository.GetSagaByCorrelationIdAsync(@event.CorrelationId, ct);
        if (saga is null) return;

        saga.Fail(@event.Reason);
        saga.Compensate();
        await _bookingRepository.UpdateAsync(saga, ct);
    }

    // ── STEP 3: Payment completed ─────────────────────────────────────────────────
    // Received from PaymentService after a successful charge.
    public async Task HandleAsync(PaymentCompletedEvent @event, CancellationToken ct = default)
    {
        var saga = await _bookingRepository.GetSagaByCorrelationIdAsync(@event.CorrelationId, ct);
        if (saga is null) return;

        saga.MarkPaymentCompleted(@event.PaymentId);
        saga.MarkFinalized();
        await _bookingRepository.UpdateAsync(saga, ct);

        await _messageClient.EnqueueAsync(
            new QueueName { Name = "booking.confirm-status" },
            new ConfirmBookingStatusCommand(
                CommandId:     Guid.NewGuid(),
                CorrelationId: saga.CorrelationId,
                BookingId:     saga.BookingId!.Value,
                IssuedAt:      DateTime.UtcNow));

        await _messageClient.PublishAsync(
            new LaunchNotificationCommand(
                CommandId:        Guid.NewGuid(),
                CorrelationId:    saga.CorrelationId,
                RecipientId:      saga.CustomerId,
                RecipientEmail:   @event.CustomerEmail,
                Subject:          "Your booking is confirmed!",
                Body:             $"Booking {saga.BookingId} has been confirmed.",
                NotificationType: NotificationType.BookingConfirmed,
                IssuedAt:         DateTime.UtcNow));
    }

    // ── STEP 3b: Payment failed (compensate) ──────────────────────────────────────
    public async Task HandleAsync(PaymentFailedEvent @event, CancellationToken ct = default)
    {
        var saga = await _bookingRepository.GetSagaByCorrelationIdAsync(@event.CorrelationId, ct);
        if (saga is null) return;

        saga.Fail(@event.Reason);
        await _bookingRepository.UpdateAsync(saga, ct);

        await _messageClient.EnqueueAsync(
            new QueueName { Name = "booking.cancel-status" },
            new UpdateBookingStatusToCancelledCommand(
                CommandId:     Guid.NewGuid(),
                CorrelationId: saga.CorrelationId,
                BookingId:     saga.BookingId!.Value,
                CustomerId:    saga.CustomerId,
                Reason:        @event.Reason,
                IssuedAt:      DateTime.UtcNow));

        saga.Compensate();
        await _bookingRepository.UpdateAsync(saga, ct);
    }
}