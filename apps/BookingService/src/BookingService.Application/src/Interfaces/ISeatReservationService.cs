using BookingService.Application.Models;

namespace BookingService.Application.Interfaces;

public interface ISeatReservationService
{
    Task<SeatClaimResult> ClaimSeatAsync(SeatBookingRequest request, CancellationToken cancellationToken = default);
    Task<SeatAvailabilityResult> CheckAvailabilityAsync(Guid eventId, string seatId, CancellationToken cancellationToken = default);
    Task ReleaseClaimAsync(Guid eventId, string seatId, Guid reservationToken, CancellationToken cancellationToken = default);
}
