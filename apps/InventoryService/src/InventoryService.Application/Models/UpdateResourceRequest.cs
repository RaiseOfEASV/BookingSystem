using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.Models;

public record UpdateResourceRequest(
    [Required] string Name,
    [Required] string Description,
    [Required] string Location,
    [Range(1, int.MaxValue)] int Capacity,
    [Range(0, double.MaxValue)] decimal PricePerUnit,
    string Currency,
    bool IsActive
);
