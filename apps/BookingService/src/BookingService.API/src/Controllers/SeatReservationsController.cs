using BookingService.Application.Interfaces;
using BookingService.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/seat-reservations")]
[Produces("application/json")]
public class SeatReservationsController : ControllerBase
{
    private readonly ISeatReservationService _reservationService;

    public SeatReservationsController(ISeatReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    /// <summary>
    /// Atomically claims a seat for the given event.
    /// The reservation token identifies the saga orchestration flow that owns the claim.
    /// Returns 409 if the seat is already held by another reservation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SeatClaimResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ClaimSeat(
        [FromBody] SeatBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _reservationService.ClaimSeatAsync(request, cancellationToken);

        if (!result.Claimed)
            return Conflict(new { message = "Seat is already reserved.", result.EventId, result.SeatId });

        return Ok(result);
    }

    /// <summary>
    /// Checks whether a seat is currently available without claiming it.
    /// </summary>
    [HttpGet("{eventId:guid}/{seatId}/availability")]
    [ProducesResponseType(typeof(SeatAvailabilityResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckAvailability(
        Guid eventId,
        string seatId,
        CancellationToken cancellationToken)
    {
        var result = await _reservationService.CheckAvailabilityAsync(eventId, seatId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Releases a seat claim. Only succeeds if the reservation token matches
    /// the one that originally claimed the seat (enforced atomically in Redis).
    /// </summary>
    [HttpDelete("{eventId:guid}/{seatId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReleaseSeat(
        Guid eventId,
        string seatId,
        [FromQuery] Guid reservationToken,
        CancellationToken cancellationToken)
    {
        await _reservationService.ReleaseClaimAsync(eventId, seatId, reservationToken, cancellationToken);
        return NoContent();
    }
}
