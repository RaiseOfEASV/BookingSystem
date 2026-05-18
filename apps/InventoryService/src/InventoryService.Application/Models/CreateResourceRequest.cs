using InventoryService.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.Models;

public record CreateResourceRequest(
    [Required] string Name,
    [Required] ResourceType Type,
    [Required] string Description,
    [Required] string Location,
    [Range(1, int.MaxValue)] int Capacity,
    [Range(0, double.MaxValue)] decimal PricePerUnit,
    string Currency = "USD"
);
