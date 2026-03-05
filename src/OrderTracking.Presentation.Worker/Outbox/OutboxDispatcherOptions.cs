namespace OrderTracking.Presentation.Worker.Outbox;

/// <summary>Configuration for <see cref="OutboxDispatcherHostedService"/> loop.</summary>
public sealed class OutboxDispatcherOptions
{
    /// <summary>Configuration section name for <see cref="OutboxDispatcherOptions"/>.</summary>
    public const string SectionName = "OutboxDispatcher";

    /// <summary>Gets or sets the batch size for processing outbox messages.</summary>
    public int BatchSize { get; init; } = 50;

    /// <summary>Gets or sets the poll interval in seconds.</summary>
    public int PollIntervalSeconds { get; init; } = 2;

    /// <summary>Gets or sets the maximum number of processing attempts before marking as poisoned.</summary>
    public int MaxAttempts { get; init; } = 10;
}
