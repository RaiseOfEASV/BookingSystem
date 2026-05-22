using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Saga.Commands;

namespace InventoryService.Infrastructure.Repositories.Postgres;

public class MessageRepository : IMessageRepository
{
    private readonly MessagesDbContext _context;

    public MessageRepository(MessagesDbContext context)
    {
        _context = context;
    }

    public async Task<Message> AddAsync(ReserveSeatCommand payload, CancellationToken cancellationToken = default)
    {
        var entity = ToEntity(payload);

        _context.Messages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ToMessage(entity);
    }

    public async Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        return entity is null ? null : ToMessage(entity);
    }

    public async Task<Message?> MarkAsProcessedAsync(Guid id,  CancellationToken cancellationToken = default)
        => await UpdateStatusAsync(id, MessageStatus.Processed, cancellationToken);

    public async Task<bool> MarkAsFailedAsync(Guid id, string error,int maxRetries, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (entity is null) return false;

        entity.RetryCount += 1;
        entity.LastError   = error;
        entity.UpdatedAt   = DateTime.UtcNow;
        entity.Status      = entity.RetryCount >= maxRetries
            ? MessageStatusMapper.ToString(MessageStatus.DeadLetter)
            : MessageStatusMapper.ToString(MessageStatus.Failed);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.RetryCount >= maxRetries;
    }

    public async Task<List<Message>> GetMessagesPending(
        int batchSize,
        int maxRetries,
        TimeSpan retryInterval,
        TimeSpan stuckTimeout,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Messages
            .FromSqlRaw(@"
                UPDATE messages
                SET status = {0}
                WHERE id IN (
                    SELECT id FROM messages
                    WHERE status = {1}
                       OR (status = {2}
                           AND retry_count < {3}
                           AND updated_at < NOW() - {6})
                       OR (status = {4}
                           AND updated_at < NOW() - {7})
                    ORDER BY received_at
                    LIMIT {5}
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING *",
                MessageStatusMapper.ToString(MessageStatus.Processing),
                MessageStatusMapper.ToString(MessageStatus.Received),
                MessageStatusMapper.ToString(MessageStatus.Failed),
                maxRetries,
                MessageStatusMapper.ToString(MessageStatus.Processing),
                batchSize,
                retryInterval,
                stuckTimeout)
            .ToListAsync(cancellationToken);

        return entities.Select(ToMessage).ToList();
    }

    private async Task<Message?> UpdateStatusAsync(Guid id, MessageStatus status, CancellationToken cancellationToken)
    {
        var entity = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (entity is null) return null;

        entity.Status      = MessageStatusMapper.ToString(status);
        entity.UpdatedAt   = DateTime.UtcNow;
        entity.ProcessedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ToMessage(entity);
    }

    private static MessageEntity ToEntity(ReserveSeatCommand payload) => new()
    {
        Id            = Guid.NewGuid(),
        EventId       = payload.EventId,
        SeatId        = payload.SeatId,
        CorrelationId = payload.CorrelationId,
        Status        = MessageStatusMapper.ToString(MessageStatus.Received),
        ReceivedAt    = DateTime.UtcNow,
        UpdatedAt     = DateTime.UtcNow,
        ProcessedAt   = null
    };

    private static Message ToMessage(MessageEntity e) => new(
        e.Id,
        new ReserveSeatCommand(e.EventId, e.SeatId, e.CorrelationId),
        Enum.Parse<MessageStatus>(e.Status, ignoreCase: true),
        e.ReceivedAt,
        e.ProcessedAt
    );
}