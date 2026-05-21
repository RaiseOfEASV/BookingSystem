using System.Text.Json;
using BookingService.Application.Options;
using BookingService.Infrastructure.Persistence;
using MessageClient.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookingService.Infrastructure.BackgroundWorkers;

public sealed class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory           _scopeFactory;
    private readonly IOptionsMonitor<OutboxWorkerOptions> _options;
    private readonly ILogger<OutboxWorker>          _logger;

    public OutboxWorker(
        IServiceScopeFactory           scopeFactory,
        IOptionsMonitor<OutboxWorkerOptions> options,
        ILogger<OutboxWorker>          logger)
    {
        _scopeFactory = scopeFactory;
        _options      = options;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken);
            await Task.Delay(_options.CurrentValue.PollingIntervalMilliseconds, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        await using var scope         = _scopeFactory.CreateAsyncScope();
        var db                        = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        var messageClient             = scope.ServiceProvider.GetRequiredService<IMessageClient>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(_options.CurrentValue.BatchSize)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                var type = ResolveType(message.Type);

                if (type is null)
                {
                    _logger.LogWarning(
                        "Outbox message {Id}: cannot resolve type '{Type}' — skipping.",
                        message.Id, message.Type);
                    continue;
                }

                var payload = JsonSerializer.Deserialize(message.Content, type)
                    ?? throw new InvalidOperationException($"Deserialized payload for message {message.Id} is null.");

                // PublishAsync<T> must be called with the concrete type resolved at runtime.
                var publishMethod = typeof(IMessageClient)
                    .GetMethod(nameof(IMessageClient.PublishAsync))!
                    .MakeGenericMethod(type);

                await (Task)publishMethod.Invoke(messageClient, [payload])!;

                message.ProcessedOn = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Outbox message {Id} of type '{Type}' published and marked as processed.",
                    message.Id, message.Type);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "Failed to process outbox message {Id} of type '{Type}'.",
                    message.Id, message.Type);
            }
        }
    }

    /// <summary>
    /// Scans all loaded assemblies for the type matching the stored full name.
    /// Works as long as the SharedContracts assembly is loaded at runtime.
    /// </summary>
    private static Type? ResolveType(string fullName) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(fullName))
            .FirstOrDefault(t => t is not null);
}