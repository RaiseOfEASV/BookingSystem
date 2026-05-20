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
    private readonly IBookingSagaRepository _sagaRepository;
    private readonly IBookingService        _bookingService;
    private readonly IMessageClient         _messageClient;

    public BookingSagaOrchestrator(
        IBookingSagaRepository sagaRepository,
        IBookingService        bookingService,
        IMessageClient         messageClient)
    {
        _sagaRepository = sagaRepository;
        _bookingService = bookingService;
        _messageClient  = messageClient;
    }

    // ── STEP 1: Trigger ───────────────────────────────────────────────────────────
    // Entry point — called by the SeatReservationsService after the Redis claim succeeds.
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

        await _sagaRepository.SaveAsync(saga, ct);
        await _messageClient.PublishAsync(new ReserveSeatCommand(command.EventId, command.SeatId, command.CorrelationId));
    }

    // ── STEP 2a: Seat reserved ────────────────────────────────────────────────────
    // Received from InventoryService after it locks the seat in its own DB.
    public async Task HandleAsync(SeatReservedEvent @event, CancellationToken ct = default)
    {
        var saga = await _sagaRepository.GetByCorrelationIdAsync(@event.CorrelationId, ct);
        if (saga is null) return;

        saga.MarkSeatReserved();
        saga.MarkCreatingBooking();
        await _sagaRepository.UpdateAsync(saga, ct);

        // Booking creation is owned by this service — call directly, no round-trip needed.
        var booking = await _bookingService.CreateAsync(
            new CreateBookingRequest(
                CustomerId: saga.CustomerId,
                EventId:    saga.EventId,
                SeatId:     saga.SeatId,
                Amount:     new Domain.ValueObjects.Price(saga.Amount, saga.Currency),
                Currency:   saga.Currency,
                Notes:      saga.Notes),
            ct);

        saga.MarkBookingCreated(booking.Id);
        saga.MarkPaymentPending();
        await _sagaRepository.UpdateAsync(saga, ct);

        await _messageClient.PublishAsync(
            new InitiatePaymentCommand(
                CommandId:     Guid.NewGuid(),
                CorrelationId: saga.CorrelationId,
                BookingId:     booking.Id,
                CustomerId:    saga.CustomerId,
                Amount:        booking.Amount,
                Currency:      booking.Currency,
                IssuedAt:      DateTime.UtcNow));
    }

    // ── STEP 2b: Seat reservation failed (compensate) ─────────────────────────────
    public async Task HandleAsync(SeatReservationFailedEvent @event, CancellationToken ct = default)
    {
        var saga = await _sagaRepository.GetByCorrelationIdAsync(@event.CorrelationId, ct);
        if (saga is null) return;

        saga.Fail(@event.Reason);
        saga.Compensate();
        await _sagaRepository.UpdateAsync(saga, ct);
    }

    // ── STEP 3: Payment completed ─────────────────────────────────────────────────
    // Received from PaymentService after a successful charge.
    public async Task HandleAsync(PaymentCompletedEvent @event, CancellationToken ct = default)
    {
        var saga = await _sagaRepository.GetByCorrelationIdAsync(@event.CorrelationId, ct);
        if (saga is null) return;

        saga.MarkPaymentCompleted(@event.PaymentId);
        saga.MarkFinalized();
        await _sagaRepository.UpdateAsync(saga, ct);

        // Flip the booking record from Pending → Confirmed.
        await _messageClient.EnqueueAsync(
            new QueueName { Name = "booking.confirm-status" },
            new ConfirmBookingStatusCommand(
                CommandId:     Guid.NewGuid(),
                CorrelationId: saga.CorrelationId,
                BookingId:     saga.BookingId!.Value,
                IssuedAt:      DateTime.UtcNow));

        // Broadcast success — any subscriber (email, push, audit) will pick this up.
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
        var saga = await _sagaRepository.GetByCorrelationIdAsync(@event.CorrelationId, ct);
        if (saga is null) return;

        saga.Fail(@event.Reason);
        await _sagaRepository.UpdateAsync(saga, ct);

        // Cancel the booking record.
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
        await _sagaRepository.UpdateAsync(saga, ct);
    }
}
// ── STEP 2a: Seat reserved (Fixed Atomicity & Flow) ───────────────────────────
public async Task HandleAsync(SeatReservedEvent @event, CancellationToken ct = default)
{
    var saga = await _sagaRepository.GetByCorrelationIdAsync(@event.CorrelationId, ct);
    if (saga is null) return;

    // Advance states internally first to track progress in memory
    saga.MarkSeatReserved();
    saga.MarkCreatingBooking();

    // Call your local boundary service
    var booking = await _bookingService.CreateAsync(
        new CreateBookingRequest(
            CustomerId: saga.CustomerId,
            EventId:    saga.EventId,
            SeatId:     saga.SeatId,
            Amount:     new Domain.ValueObjects.Price(saga.Amount, saga.Currency),
            Currency:   saga.Currency,
            Notes:      saga.Notes),
        ct);

    // Continue shifting the status safely before committing a single DB write
    saga.MarkBookingCreated(booking.Id);
    saga.MarkPaymentPending();
    
    // ONE atomic save representing the completion of this handler execution loop
    await _sagaRepository.UpdateAsync(saga, ct);

    await _messageClient.PublishAsync(
        new InitiatePaymentCommand(
            CommandId:     Guid.NewGuid(),
            CorrelationId: saga.CorrelationId,
            BookingId:     booking.Id,
            CustomerId:    saga.CustomerId,
            Amount:        booking.Amount,
            Currency:      booking.Currency,
            IssuedAt:      DateTime.UtcNow));
}

// ── STEP 3b: Payment failed (Fixed Inventory Leak) ─────────────────────────────
public async Task HandleAsync(PaymentFailedEvent @event, CancellationToken ct = default)
{
    var saga = await _sagaRepository.GetByCorrelationIdAsync(@event.CorrelationId, ct);
    if (saga is null) return;

    saga.Fail(@event.Reason);

    // 1. Cancel the local booking record
    await _messageClient.EnqueueAsync(
        new QueueName { Name = "booking.cancel-status" },
        new UpdateBookingStatusToCancelledCommand(
            CommandId:     Guid.NewGuid(),
            CorrelationId: saga.CorrelationId,
            BookingId:     saga.BookingId!.Value,
            CustomerId:    saga.CustomerId,
            Reason:        @event.Reason,
            IssuedAt:      DateTime.UtcNow));

    // 2. FIX: Release the seat back to inventory so it can be booked by others!
    await _messageClient.PublishAsync(
        new ReleaseSeatCommand(
            EventId:       saga.EventId, 
            SeatId:        saga.SeatId, 
            CorrelationId: saga.CorrelationId));

    saga.Compensate();
    await _sagaRepository.UpdateAsync(saga, ct);
}