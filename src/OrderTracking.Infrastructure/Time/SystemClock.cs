using OrderTracking.Application.Abstractions.Time;

namespace OrderTracking.Infrastructure.Time;

/// <summary>
/// Provides the current UTC time.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
