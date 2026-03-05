namespace OrderTracking.Application.Abstractions.Time;

/// <summary>Time provider for deterministic business logic and tests.</summary>
public interface IClock
{
    /// <summary>Current UTC time.</summary>
    DateTimeOffset UtcNow { get; }
}
