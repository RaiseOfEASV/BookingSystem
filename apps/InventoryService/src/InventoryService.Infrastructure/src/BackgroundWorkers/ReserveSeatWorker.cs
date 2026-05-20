using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedContracts.Saga.Commands;

namespace InventoryService.Infrastructure.BackgroundWorkers;

public sealed class ReserveSeatWorker : BackgroundService
{
    private readonly IServiceScopeFactory       _scopeFactory;
    private readonly ILogger<ReserveSeatWorker> _logger;
    private readonly IOptionsMonitor<MessageProcessingOptions> _options;
    


    public ReserveSeatWorker(IServiceScopeFactory scopeFactory, ILogger<ReserveSeatWorker> logger,IOptionsMonitor<MessageProcessingOptions> options)
    {
        _options = options;
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReserveSeatWorker started.");

        var currentDelay = TimeSpan.FromMilliseconds(_options.CurrentValue.MinPollingDelayMs);

        while (!stoppingToken.IsCancellationRequested)
        {
            var minDelay = TimeSpan.FromMilliseconds(_options.CurrentValue.MinPollingDelayMs);
            var maxDelay = TimeSpan.FromMilliseconds(_options.CurrentValue.MaxPollingDelayMs);

            try
            {
                var processed = await ProcessPendingMessagesAsync(stoppingToken);

                if (processed == 0)
                    currentDelay = currentDelay * 2 > maxDelay ? maxDelay : currentDelay * 2;
                else
                    currentDelay = processed == _options.CurrentValue.BatchSize ? TimeSpan.Zero : minDelay;

                if (currentDelay > TimeSpan.Zero)
                    await Task.Delay(currentDelay, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unhandled error in ReserveSeatWorker polling loop.");
                await Task.Delay(maxDelay, stoppingToken);
            }
        }

        _logger.LogInformation("ReserveSeatWorker stopped.");
    }

    private async Task<int> ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope      = _scopeFactory.CreateAsyncScope();
        var reserveSeatService     = scope.ServiceProvider.GetRequiredService<IReserveSeatService>();
        var messageRepository      = scope.ServiceProvider.GetRequiredService<IMessageRepository>();

        var pending = await messageRepository.GetMessagesPending(
            _options.CurrentValue.BatchSize,
            _options.CurrentValue.MaxRetries,
            TimeSpan.FromSeconds(_options.CurrentValue.RetryIntervalSeconds),
            TimeSpan.FromSeconds(_options.CurrentValue.StuckTimeoutSeconds),
            cancellationToken);        
        if (pending.Count == 0) return 0;

        _logger.LogInformation("Processing {Count} pending reserve-seat message(s).", pending.Count);

        foreach (var message in pending)
        {
            try
            {
                var command = new ReserveSeatCommand(message.Payload.EventId, message.Payload.SeatId, message.Payload.CorrelationId);
                await reserveSeatService.HandleReserveSeat(command, cancellationToken);
                await messageRepository.MarkAsProcessedAsync(message.Id, cancellationToken);
                _logger.LogInformation("Message {Id} processed successfully.", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {Id}.", message.Id);
                try
                {
                    var isDeadLettered = await messageRepository.MarkAsFailedAsync(
                        message.Id, ex.Message, _options.CurrentValue.MaxRetries, cancellationToken);

                    if (isDeadLettered)
                    {
                        try
                        {
                            var reason = "Message processing retry have reached maximum, message will be aborted";
                            await reserveSeatService.HandleMessageProcessingFailed(
                                new SeatReservationFailedEvent(message.Payload.EventId,message.Payload.SeatId,message.Payload.CorrelationId,reason,SeatReservationFailureCode.ServerError), cancellationToken);
                        }
                        catch (Exception publishEx)
                        {
                            _logger.LogError(publishEx,
                                "Dead-letter notification failed for message {Id}. " +
                                "Message is dead-lettered in DB but saga was not notified.", message.Id);
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx,
                        "Failed to mark message {Id} as failed. " +
                        "It will be recovered by stuck-processing after the timeout.", message.Id);
                }
            }
        }

        return pending.Count;
    }
}