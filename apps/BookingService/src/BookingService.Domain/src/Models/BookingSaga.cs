namespace BookingService.Domain.Models;

public class BookingSaga
{
    public Guid     Id            { get; init; }
    public Guid     EventId       { get; init; }
    public Guid     SeatId        { get; init; }
    public Guid     CustomerId    { get; init; }
    public decimal  Amount        { get; init; }
    public string   Currency      { get; init; } = null!;
    public string?  Notes         { get; init; }
    public Guid?    BookingId     { get; private set; }
    public Guid     CorrelationId { get; init; }
    public string?  PaymentId     { get; private set; }
    public SagaStatus Status      { get; private set; }
    public string?  FailureReason { get; private set; }
    public DateTime CreatedAt     { get; init; }
    public DateTime UpdatedAt     { get; private set; }

    public static BookingSaga Create(
        Guid    eventId,
        Guid    seatId,
        Guid    customerId,
        decimal amount,
        string  currency,
        string? notes,
        Guid    correlationId)
    {
        return new BookingSaga
        {
            Id            = Guid.NewGuid(),
            EventId       = eventId,
            SeatId        = seatId,
            CustomerId    = customerId,
            Amount        = amount,
            Currency      = currency,
            Notes         = notes,
            CorrelationId = correlationId,
            Status        = SagaStatus.CheckingSeatAvailability,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow
        };
    }

    public static BookingSaga Restore(
        Guid      id,
        Guid      eventId,
        Guid      seatId,
        Guid      customerId,
        decimal   amount,
        string    currency,
        string?   notes,
        Guid?     bookingId,
        Guid      correlationId,
        string?   paymentId,
        SagaStatus status,
        string?   failureReason,
        DateTime  createdAt,
        DateTime  updatedAt)
    {
        return new BookingSaga
        {
            Id            = id,
            EventId       = eventId,
            SeatId        = seatId,
            CustomerId    = customerId,
            Amount        = amount,
            Currency      = currency,
            Notes         = notes,
            BookingId     = bookingId,
            CorrelationId = correlationId,
            PaymentId     = paymentId,
            Status        = status,
            FailureReason = failureReason,
            CreatedAt     = createdAt,
            UpdatedAt     = updatedAt
        };
    }

    public void MarkSeatReserved()
    {
        Transition(SagaStatus.CheckingSeatAvailability, SagaStatus.SeatReserved);
    }

    public void MarkCreatingBooking()
    {
        Transition(SagaStatus.SeatReserved, SagaStatus.CreatingBooking);
    }

    public void MarkBookingCreated(Guid bookingId)
    {
        Transition(SagaStatus.CreatingBooking, SagaStatus.BookingCreated);
        BookingId = bookingId;
    }

    public void MarkPaymentPending()
    {
        Transition(SagaStatus.BookingCreated, SagaStatus.PaymentPending);
    }

    public void MarkPaymentCompleted(string paymentId)
    {
        Transition(SagaStatus.PaymentPending, SagaStatus.PaymentCompleted);
        PaymentId = paymentId;
    }

    public void MarkFinalized()
    {
        Transition(SagaStatus.PaymentCompleted, SagaStatus.Finalized);
    }

    public void Fail(string reason)
    {
        FailureReason = reason;
        Status = SagaStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Compensate()
    {
        Status = SagaStatus.Compensated;
        UpdatedAt = DateTime.UtcNow;
    }

    private void Transition(SagaStatus expected, SagaStatus next)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Cannot transition to {next}. Expected status {expected} but was {Status}.");

        Status = next;
        UpdatedAt = DateTime.UtcNow;
    }
}
