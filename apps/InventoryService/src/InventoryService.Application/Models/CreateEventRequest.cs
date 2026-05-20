namespace InventoryService.Application.Models;

public record CreateEventRequest(
    Guid     VenueId,
    string   EventName,
    DateTime StartDate,
    DateTime EndDate
);
