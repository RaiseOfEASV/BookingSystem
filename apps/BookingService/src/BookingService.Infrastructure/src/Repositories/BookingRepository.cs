using BookingService.Application.Exceptions;
using BookingService.Application.Interfaces;
using BookingService.Application.Options;
using BookingService.Domain.Entities;
using BookingService.Domain.Models;
using BookingService.Domain.ValueObjects;
using BookingService.Infrastructure.Persistence;
using BookingService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data.Common;
using BookingService.Application.Models;

namespace BookingService.Infrastructure.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;
    private readonly IOptionsMonitor<BookingServiceOptions> _options;

    public BookingRepository(BookingDbContext context, IOptionsMonitor<BookingServiceOptions> options)
    {
        _context = context;
        _options = options;
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<Booking?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.CorrelationId == correlationId, cancellationToken);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var opts = _options.CurrentValue;
        Exception? lastException = null;

        for (int attempt = 0; attempt <= opts.MaxRetries; attempt++)
        {
            var entity = ToEntity(booking);
            _context.Bookings.Add(entity);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return ToDomain(entity);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new DuplicateBookingException();
            }
            catch (Exception ex) when (ex is not OperationCanceledException && IsTransient(ex))
            {
                _context.ChangeTracker.Clear();
                lastException = ex;

                if (attempt < opts.MaxRetries)
                {
                    var delayMs = Math.Min(
                        opts.RetryDelayMilliseconds * (long)Math.Pow(2, attempt),
                        opts.MaxDelayMilliseconds);

                    await Task.Delay((int)delayMs, cancellationToken);
                }
            }
        }

        throw new BookingPersistenceException("Failed to persist booking after retries exhausted.", lastException);
    }

    private static bool IsTransient(Exception ex) =>
        ex is TimeoutException
          or IOException
          or System.Net.Sockets.SocketException
          or DbException { IsTransient: true };

    public async Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == booking.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Booking {booking.Id} not found.");

        entity.Status    = booking.Status.ToString();
        entity.Notes     = booking.Notes;
        entity.UpdatedAt = booking.UpdatedAt;

        await _context.SaveChangesAsync(cancellationToken);

        return ToDomain(entity);
    }

    public async Task<BookingSaga?> GetSagaByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.BookingSagas
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CorrelationId == correlationId, cancellationToken);

        return entity is null ? null : ToSagaDomain(entity);
    }

    public async Task SaveAsync(BookingSaga saga, OutboxMessageDto messageDto, CancellationToken cancellationToken = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.BookingSagas.Add(ToSagaEntity(saga));
        _context.OutboxMessages.Add(ToOutboxEntity(messageDto));

        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    public async Task UpdateAsync(BookingSaga saga, CancellationToken cancellationToken = default)
    {
        var entity = await _context.BookingSagas
            .FirstOrDefaultAsync(s => s.Id == saga.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"BookingSaga {saga.Id} not found.");

        entity.Status        = saga.Status.ToString();
        entity.BookingId     = saga.BookingId;
        entity.PaymentId     = saga.PaymentId;
        entity.FailureReason = saga.FailureReason;
        entity.UpdatedAt     = saga.UpdatedAt;

        await _context.SaveChangesAsync(cancellationToken);
    }

    // ── Mapping ───────────────────────────────────────────────────────────────────

    private static BookingEntity ToEntity(Booking b) => new()
    {
        Id            = b.Id,
        CorrelationId = b.CorrelationId,
        CustomerId    = b.CustomerId,
        EventId       = b.EventId,
        SeatId        = b.SeatId,
        Amount        = b.Price.Amount,
        Currency      = b.Price.Currency,
        Status        = b.Status.ToString(),
        Code          = b.Code.Value,
        Notes         = b.Notes,
        CreatedAt     = b.CreatedAt,
        UpdatedAt     = b.UpdatedAt
    };

    private static Booking ToDomain(BookingEntity e) => Booking.Restore(
        id:            e.Id,
        correlationId: e.CorrelationId,
        customerId:    e.CustomerId,
        eventId:       e.EventId,
        seatId:        e.SeatId,
        price:         new Price(e.Amount, e.Currency),
        status:        Enum.Parse<BookingStatus>(e.Status),
        code:          BookingCode.From(e.Code),
        notes:         e.Notes,
        createdAt:     e.CreatedAt,
        updatedAt:     e.UpdatedAt);

    private static BookingSagaEntity ToSagaEntity(BookingSaga s) => new()
    {
        Id            = s.Id,
        EventId       = s.EventId,
        SeatId        = s.SeatId,
        CustomerId    = s.CustomerId,
        Amount        = s.Amount,
        Currency      = s.Currency,
        Notes         = s.Notes,
        BookingId     = s.BookingId,
        CorrelationId = s.CorrelationId,
        PaymentId     = s.PaymentId,
        Status        = s.Status.ToString(),
        FailureReason = s.FailureReason,
        CreatedAt     = s.CreatedAt,
        UpdatedAt     = s.UpdatedAt
    };

    private static BookingSaga ToSagaDomain(BookingSagaEntity e) => BookingSaga.Restore(
        id:            e.Id,
        eventId:       e.EventId,
        seatId:        e.SeatId,
        customerId:    e.CustomerId,
        amount:        e.Amount,
        currency:      e.Currency,
        notes:         e.Notes,
        bookingId:     e.BookingId,
        correlationId: e.CorrelationId,
        paymentId:     e.PaymentId,
        status:        Enum.Parse<SagaStatus>(e.Status),
        failureReason: e.FailureReason,
        createdAt:     e.CreatedAt,
        updatedAt:     e.UpdatedAt);

    private static OutboxMessageEntity ToOutboxEntity(OutboxMessageDto m) => new()
    {
        Id          = m.Id,
        OccurredOn  = m.OccurredOn,
        Type        = m.Type,
        Content     = m.Content,
        ProcessedOn = m.ProcessedOn
    };
}
