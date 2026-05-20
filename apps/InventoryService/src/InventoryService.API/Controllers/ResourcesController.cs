using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IEventService _service;

    public EventsController(IEventService service)
    {
        _service = service;
    }

    /// <summary>Gets all events.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var events = await _service.GetAllAsync(cancellationToken);
        return Ok(events);
    }

    /// <summary>Gets a single event by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ev = await _service.GetByIdAsync(id, cancellationToken);
        return ev is null ? NotFound() : Ok(ev);
    }

    /// <summary>Gets all available seats for an event.</summary>
    [HttpGet("{id:guid}/available-seats")]
    [ProducesResponseType(typeof(IEnumerable<AvailableSeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailableSeats(Guid id, CancellationToken cancellationToken)
    {
        var ev = await _service.GetByIdAsync(id, cancellationToken);
        if (ev is null) return NotFound();

        var seats = await _service.GetAvailableSeatsAsync(id, cancellationToken);
        return Ok(seats);
    }

    /// <summary>Creates a new event.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request, CancellationToken cancellationToken)
    {
        var ev = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ev);
    }

    /// <summary>Updates an existing event.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var ev = await _service.UpdateAsync(id, request, cancellationToken);
        return ev is null ? NotFound() : Ok(ev);
    }

    /// <summary>Deletes an event by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
