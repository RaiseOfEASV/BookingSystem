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

    /// <summary>Gets all bookings.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var bookings = await _service.GetAllAsync(cancellationToken);
        return Ok(bookings);
    }

    /// <summary>Gets a booking by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _service.GetByIdAsync(id, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Gets all bookings for a customer.</summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var bookings = await _service.GetByCustomerIdAsync(customerId, cancellationToken);
        return Ok(bookings);
    }

    /// <summary>Gets all bookings that include a specific resource.</summary>
    [HttpGet("resource/{resourceId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<BookingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByResource(Guid resourceId, CancellationToken cancellationToken)
    {
        var bookings = await _service.GetByResourceIdAsync(resourceId, cancellationToken);
        return Ok(bookings);
    }

    /// <summary>
    /// Creates a new booking with one or more resource items.
    /// Each item has its own resource, time window, quantity and price.
    /// Example: 2 cinema tickets 
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var invalidItem = request.Items.FirstOrDefault(i => i.StartTime >= i.EndTime);
        if (invalidItem is not null)
            return BadRequest($"Item '{invalidItem.ResourceType}' at '{invalidItem.ResourceLocation}': StartTime must be before EndTime.");

        var booking = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    /// <summary>
    /// Replaces all items on an existing booking and recalculates the total.
    /// Cannot update a Cancelled or Completed booking.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingRequest request, CancellationToken cancellationToken)
    {
        var invalidItem = request.Items.FirstOrDefault(i => i.StartTime >= i.EndTime);
        if (invalidItem is not null)
            return BadRequest($"Item '{invalidItem.ResourceType}' at '{invalidItem.ResourceLocation}': StartTime must be before EndTime.");

        var booking = await _service.UpdateAsync(id, request, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Cancels a booking.</summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _service.CancelAsync(id, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }

    /// <summary>Deletes a booking and all its items permanently.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
