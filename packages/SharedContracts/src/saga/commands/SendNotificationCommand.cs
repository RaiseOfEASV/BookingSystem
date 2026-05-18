namespace SharedContracts.Saga.Commands;

public record LaunchNotificationCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid RecipientId,
    string RecipientEmail,
    string Subject,
    string Body,
    NotificationType NotificationType,
    DateTime IssuedAt
);

public enum NotificationType
{
    BookingConfirmed = 0,
    BookingCancelled = 1,
    PaymentSucceeded = 2,
    PaymentFailed = 3
}
