using BookingService.Domain.Entities;

namespace BookingService.Application.Models;

public record BookingDto(
    Guid Id,
    Guid CustomerId,
    BookingStatus Status,
    string StatusName,
    decimal TotalPrice,
    string? Notes,
    IReadOnlyList<BookingItemDto> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record BookingItemDto(
    Guid Id,
    Guid ResourceId,
    ResourceType ResourceType,
    string ResourceTypeName,
    string ResourceLocation,
    DateTime StartTime,
    DateTime EndTime,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal
);
