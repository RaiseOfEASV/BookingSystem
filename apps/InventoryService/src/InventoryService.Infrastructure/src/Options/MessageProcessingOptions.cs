namespace InventoryService.Infrastructure.Options;

public sealed class MessageProcessingOptions
{
    public const string SectionName = "MessageProcessing";
    public int BatchSize  { get; init; } = 10;  
    public int MaxRetries { get; init; } = 3;
    public int RetryIntervalSeconds { get; set; } = 30;
    public int StuckTimeoutSeconds  { get; set; } = 30;
    public int MinPollingDelayMs    { get; set; } = 100;
    public int MaxPollingDelayMs    { get; set; } = 500;
}