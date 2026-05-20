namespace InventoryService.Application.Models;

public record EventDto(
    Guid     Id,
    Guid     VenueId,
    string   EventName,
    DateTime StartDate,
    DateTime EndDate
);
