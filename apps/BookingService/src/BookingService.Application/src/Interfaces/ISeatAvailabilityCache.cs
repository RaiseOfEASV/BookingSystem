namespace BookingService.Application.Interfaces;

public interface ISeatAvailabilityCache
{
    /// <summary>
    /// Returns true if no active claim exists for this seat in the given event.
    /// </summary>
    Task<bool> IsSeatAvailableAsync(Guid eventId, string seatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically claims the seat for the given saga.
    /// Returns true if the claim was won; false if another saga already holds it.
    /// </summary>
    Task<bool> TryClaimSeatAsync(Guid eventId, string seatId, Guid reservationToken , CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases the claim only if the sagaId matches the stored value (atomic check-and-delete).
    /// </summary>
    Task ReleaseClaimAsync(Guid eventId, string seatId, Guid reservationToken, CancellationToken cancellationToken = default);
}
