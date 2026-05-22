using InventoryService.Application.Interfaces;
using MessageClient.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.Saga.Commands;

namespace InventoryService.API.Controllers;

/// <summary>
/// Development-only controller to manually trigger saga commands
/// without needing a running message broker.
/// </summary>
[ApiController]
[Route("api/saga-mock")]
[Produces("application/json")]
public class SagaMockController : ControllerBase
{
    private readonly IMessageClient _messageClient;
    private readonly IWebHostEnvironment      _env;

    public SagaMockController(IMessageClient messageClient, IWebHostEnvironment env)
    {
        _messageClient = messageClient;
        _env           = env;
    }

    /// <summary>
    /// Simulates the saga sending a ReserveSeatCommand to this service.
    /// The handler will attempt to hold the seat and publish SeatReservedEvent
    /// or SeatReservationFailedEvent to the message broker.
    /// </summary>
    [HttpPost("reserve-seat")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReserveSeat(
        [FromBody] ReserveSeatRequest request,
        CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return Forbid();

        var command = new ReserveSeatCommand(
            request.EventId,
            request.SeatId,
            request.CorrelationId ?? Guid.NewGuid());

        await _messageClient.PublishAsync<ReserveSeatCommand>(command);

        return Accepted(new
        {
            message       = "ReserveSeatCommand dispatched to handler.",
            command.EventId,
            command.SeatId,
            command.CorrelationId
        });
    }
}

public record ReserveSeatRequest(
    Guid  EventId,
    Guid  SeatId,
    Guid? CorrelationId
);
