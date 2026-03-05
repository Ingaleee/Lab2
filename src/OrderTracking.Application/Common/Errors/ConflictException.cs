namespace OrderTracking.Application.Common.Errors;

/// <summary>Thrown when a command cannot be executed due to a conflict (e.g., invalid status transition).</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
