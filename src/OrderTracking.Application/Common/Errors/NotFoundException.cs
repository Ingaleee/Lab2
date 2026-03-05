namespace OrderTracking.Application.Common.Errors;

/// <summary>Thrown when requested entity was not found.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
