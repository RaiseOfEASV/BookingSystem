namespace InventoryService.Application.Models;

public record AvailableSeatDto(
    Guid   SeatId,
    Guid   EventId,
    string Row,
    int    Number,
    string Section,
    string Status
);
