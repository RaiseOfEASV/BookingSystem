using BookingService.Application.Interfaces;
using BookingService.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    /// <summary>
    /// Initiates a new booking via the saga orchestrator.
    /// Returns the booking ID on success or a reason on failure.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { result.Failure.Reason });

        return Ok(new { result.Success.BookingId, result.Success.CorrelationId });
    }

    /// <summary>Cancels a booking.</summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _service.CancelAsync(id, cancellationToken);
            return Ok(booking);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Confirms a booking.</summary>
    [HttpPatch("{id:guid}/confirm")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _service.ConfirmAsync(id, cancellationToken);
            return Ok(booking);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
