namespace BookingService.Domain.Models;

public static class SagaStatusConverter
{
    private static readonly Dictionary<SagaStatus, string> ToStringMap = new()
    {
        [SagaStatus.CheckingSeatAvailability] = "checking_seat_availability",
        [SagaStatus.SeatReserved]             = "seat_reserved",
        [SagaStatus.CreatingBooking]          = "creating_booking",
        [SagaStatus.BookingCreated]           = "booking_created",
        [SagaStatus.PaymentPending]           = "payment_pending",
        [SagaStatus.PaymentCompleted]         = "payment_completed",
        [SagaStatus.Finalized]                = "finalized",
        [SagaStatus.Failed]                   = "failed",
        [SagaStatus.Compensated]              = "compensated"
    };

    private static readonly Dictionary<string, SagaStatus> FromStringMap =
        ToStringMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static string ToString(SagaStatus status)
    {
        if (!ToStringMap.TryGetValue(status, out var value))
            throw new ArgumentOutOfRangeException(nameof(status), $"Unknown SagaStatus: {status}");
        return value;
    }

    public static SagaStatus FromString(string value)
    {
        if (!FromStringMap.TryGetValue(value, out var status))
            throw new ArgumentOutOfRangeException(nameof(value), $"Unknown SagaStatus string: '{value}'");
        return status;
    }
}
