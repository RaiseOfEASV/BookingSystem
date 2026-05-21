namespace BookingService.Application.Models;

public record OutboxMessageDto(
    Guid      Id,
    DateTime  OccurredOn,
    string    Type,
    string    Content,
    DateTime? ProcessedOn
);