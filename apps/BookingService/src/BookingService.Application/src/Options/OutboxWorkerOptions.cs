namespace BookingService.Application.Options;

public sealed class OutboxWorkerOptions
{
    public const string SectionName = "OutboxWorker";

    /// <summary>How often the worker polls for unprocessed outbox messages, in milliseconds.</summary>
    public int PollingIntervalMilliseconds { get; init; } = 5000;

    /// <summary>Maximum number of messages processed per polling cycle.</summary>
    public int BatchSize { get; init; } = 20;
}