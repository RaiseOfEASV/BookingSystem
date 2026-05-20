using BookingService.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.Models;

public record UpdateBookingRequest(
    string? Notes,
    [Required][MinLength(1)] List<UpdateBookingItemRequest> Items
);

public record UpdateBookingItemRequest(
    [Required] Guid ResourceId,
    [Required] ResourceType ResourceType,
    [Required] string ResourceLocation,
    [Required] DateTime StartTime,
    [Required] DateTime EndTime,
    [Range(1, int.MaxValue)] int Quantity,
    [Range(0, double.MaxValue)] decimal UnitPrice
);
