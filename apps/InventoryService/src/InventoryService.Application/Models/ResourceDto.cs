using InventoryService.Domain.Entities;

namespace InventoryService.Application.Models;

public record ResourceDto(
    Guid Id,
    string Name,
    ResourceType Type,
    string TypeName,
    string Description,
    string Location,
    int Capacity,
    decimal PricePerUnit,
    string Currency,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
