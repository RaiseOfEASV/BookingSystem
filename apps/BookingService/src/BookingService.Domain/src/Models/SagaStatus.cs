namespace BookingService.Domain.Models;

public enum SagaStatus
{
    CheckingSeatAvailability,
    SeatReserved,
    CreatingBooking,
    BookingCreated,
    PaymentPending,
    PaymentCompleted,
    Finalized,
    Failed,
    Compensated
}
