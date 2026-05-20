using BookingService.Application.Interfaces;
using StackExchange.Redis;

namespace BookingService.Infrastructure.Caching;

public sealed class SeatAvailabilityCache : ISeatAvailabilityCache
{
    private static readonly TimeSpan ClaimTtl = TimeSpan.FromMinutes(10);
    private readonly IDatabase _redisDb;

    public SeatAvailabilityCache(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    // ── Key scheme ──────────────────────────────────────────────────────────────
    // seat:lock:{eventId}:{seatId} → temporary claim (10 min TTL), value = reservationToken
    private static string BuildLockKey(Guid eventId, string seatId) =>
        $"seat:lock:{eventId}:{seatId}";

    // ── ISeatAvailabilityCache ───────────────────────────────────────────────────

    public async Task<bool> IsSeatAvailableAsync(
        Guid eventId,
        string seatId,
        CancellationToken cancellationToken = default)
    {
        // Available at cache level if no active lock exists. 
        // (Permanent booking check happens when this request hits SQL right after)
        var lockKey = BuildLockKey(eventId, seatId);
        return !await _redisDb.KeyExistsAsync(lockKey);
    }

    public async Task<bool> TryClaimSeatAsync(
        Guid eventId,
        string seatId,
        Guid reservationToken, // Your idempotent token
        CancellationToken cancellationToken = default)
    {
        var lockKey = BuildLockKey(eventId, seatId);
        var tokenString = reservationToken.ToString();

        // 1. First, check who currently owns the lock (if anyone)
        var currentLockValue = await _redisDb.StringGetAsync(lockKey);

        if (currentLockValue.HasValue)
        {
            // IDEMPOTENCY RETRY: If the lock exists and matches OUR token, 
            // it's a network retry. Let it pass through successfully!
            return currentLockValue == tokenString;
        }

        // 2. No lock exists? Attempt to claim it atomically using the NX flag
        bool claimed = await _redisDb.StringSetAsync(
            lockKey,
            tokenString,
            expiry: ClaimTtl,
            when: When.NotExists);

        return claimed;
    }

    public async Task ReleaseClaimAsync(
        Guid eventId,
        string seatId,
        Guid reservationToken,
        CancellationToken cancellationToken = default)
    {
        var lockKey = BuildLockKey(eventId, seatId);

        // Open an atomic block — ONLY delete the lock if it belongs to our token
        var tran = _redisDb.CreateTransaction();
        tran.AddCondition(Condition.StringEqual(lockKey, reservationToken.ToString()));
        _ = tran.KeyDeleteAsync(lockKey);

        await tran.ExecuteAsync();
    }
}