namespace InventoryService.Application.Models;

public record AvailabilityDto(
    Guid ResourceId,
    string ResourceName,
    DateTime StartTime,
    DateTime EndTime,
    int TotalCapacity,
    int BookedUnits,
    int AvailableUnits,
    bool IsAvailable
);
