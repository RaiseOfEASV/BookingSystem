namespace BookingService.Application.Options;

public sealed class BookingServiceOptions
{
    public const string SectionName = "BookingService";

    /// <summary>Number of retry attempts on transient infrastructure failures (0 = no retries).</summary>
    public int MaxRetries { get; init; } = 3;
    

    /// <summary>Base delay in milliseconds between retries. Each attempt doubles the delay (exponential back-off).</summary>
    public int RetryDelayMilliseconds { get; init; } = 200;

    public int MaxDelayMilliseconds { get; init; } = 500;
}