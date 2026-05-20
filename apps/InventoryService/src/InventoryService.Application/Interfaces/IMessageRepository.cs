using Exceptions;
using SharedContracts.Saga.Commands;

namespace InventoryService.Application.Interfaces;

public enum MessageStatus
{
    Received,
    Processing,
    Processed,
    Failed,
    DeadLetter,
}

public record Message(
    Guid Id,
    ReserveSeatCommand Payload,
    MessageStatus Status,
    DateTime ReceivedAt,
    DateTime? ProcessedAt
);

public static class MessageStatusMapper
{
    private static readonly Dictionary<string, MessageStatus> _fromString = new()
    {
        { "received",   MessageStatus.Received   },
        { "processing", MessageStatus.Processing },
        { "processed",  MessageStatus.Processed  },
        { "failed",     MessageStatus.Failed     },
        { "deadletter", MessageStatus.DeadLetter }
    };

    public static MessageStatus FromString(string status)
    {
        if (_fromString.TryGetValue(status.ToLower(), out var result))
            return result;

        throw new DomainException($"Unknown message status: '{status}'");
    }

    public static string ToString(MessageStatus status) => status.ToString().ToLower();
}

public interface IMessageRepository
{
    Task<Message>  AddAsync(ReserveSeatCommand payload, CancellationToken cancellationToken = default);
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Message?> MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> MarkAsFailedAsync(Guid id, string error, int maxRetries, CancellationToken cancellationToken = default);
    Task<List<Message>> GetMessagesPending(int batchSize, int maxRetries, TimeSpan retryInterval, TimeSpan stuckTimeout, CancellationToken cancellationToken = default);
}
