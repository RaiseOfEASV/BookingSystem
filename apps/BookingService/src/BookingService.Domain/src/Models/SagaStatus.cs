namespace BookingService.Domain.Models;

public enum SagaStatus
{
    
    //commands
    CheckingSeatAvailability,
    SeatReserved,
    CreatingBooking,
    BookingCreated,
    PaymentPending,
    PaymentCompleted,
    
    // compensate
    ReleasingSeat,
    SeatReleased,
    CancelingBooking,
    BookingCanceled,
    Finalized,
    Failed,
    Compensated
}
