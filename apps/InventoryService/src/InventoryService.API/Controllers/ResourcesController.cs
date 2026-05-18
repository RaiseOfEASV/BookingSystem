using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using InventoryService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _service;

    public ResourcesController(IResourceService service)
    {
        _service = service;
    }

    /// <summary>Gets all active resources, optionally filtered by type.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ResourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ResourceType? type, CancellationToken cancellationToken)
    {
        var resources = type.HasValue
            ? await _service.GetByTypeAsync(type.Value, cancellationToken)
            : await _service.GetAllAsync(cancellationToken);
        return Ok(resources);
    }

    /// <summary>Gets a single resource by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _service.GetByIdAsync(id, cancellationToken);
        return resource is null ? NotFound() : Ok(resource);
    }

    /// <summary>Checks availability for a resource within a time window.</summary>
    [HttpGet("{id:guid}/availability")]
    [ProducesResponseType(typeof(AvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckAvailability(
        Guid id,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        CancellationToken cancellationToken)
    {
        if (startTime >= endTime)
            return BadRequest("startTime must be before endTime.");

        var availability = await _service.CheckAvailabilityAsync(id, startTime, endTime, cancellationToken);
        return availability is null ? NotFound() : Ok(availability);
    }

    /// <summary>Creates a new resource.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateResourceRequest request, CancellationToken cancellationToken)
    {
        var resource = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource);
    }

    /// <summary>Updates an existing resource.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResourceRequest request, CancellationToken cancellationToken)
    {
        var resource = await _service.UpdateAsync(id, request, cancellationToken);
        return resource is null ? NotFound() : Ok(resource);
    }

    /// <summary>Deletes a resource by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
