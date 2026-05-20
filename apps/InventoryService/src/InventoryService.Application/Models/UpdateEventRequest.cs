namespace InventoryService.Application.Models;

public record UpdateEventRequest(
    string   EventName,
    DateTime StartDate,
    DateTime EndDate
);
