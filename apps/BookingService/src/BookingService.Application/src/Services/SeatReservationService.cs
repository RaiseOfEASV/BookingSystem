using BookingService.Application.Interfaces;
using BookingService.Application.Models;

namespace BookingService.Application.Services;

public sealed class SeatReservationService : ISeatReservationService
{
    private readonly ISeatAvailabilityCache _cache;

    public SeatReservationService(ISeatAvailabilityCache cache)
    {
        _cache = cache;
    }

    
    public async Task<SeatClaimResult> ClaimSeatAsync(
        SeatBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        bool claimed = await _cache.TryClaimSeatAsync(
            request.EventId,
            request.SeatId,
            request.ReservationToken,
            cancellationToken);

        return new SeatClaimResult(
            request.EventId,
            request.SeatId,
            request.ReservationToken,
            claimed);
    }

    public async Task<SeatAvailabilityResult> CheckAvailabilityAsync(
        Guid eventId,
        string seatId,
        CancellationToken cancellationToken = default)
    {
        bool available = await _cache.IsSeatAvailableAsync(eventId, seatId, cancellationToken);

        return new SeatAvailabilityResult(eventId, seatId, available);
    }

    public Task ReleaseClaimAsync(
        Guid eventId,
        string seatId,
        Guid reservationToken,
        CancellationToken cancellationToken = default)
    {
        return _cache.ReleaseClaimAsync(eventId, seatId, reservationToken, cancellationToken);
    }
}
