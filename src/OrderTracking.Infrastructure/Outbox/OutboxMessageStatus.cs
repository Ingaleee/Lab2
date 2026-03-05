namespace OrderTracking.Infrastructure.Outbox;

/// <summary>Status of an <see cref="OutboxMessage"/> processing lifecycle.</summary>
public enum OutboxMessageStatus
{
    /// <summary>Message is pending processing.</summary>
    Pending = 0,

    /// <summary>Message has been successfully processed.</summary>
    Processed = 1,

    /// <summary>Message failed processing after multiple attempts.</summary>
    Poisoned = 2
}
