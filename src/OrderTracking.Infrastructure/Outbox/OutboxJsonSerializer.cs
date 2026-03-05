using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderTracking.Infrastructure.Outbox;

/// <summary>Shared JSON serializer settings for <see cref="OutboxMessage"/> payloads.</summary>
public static class OutboxJsonSerializer
{
    /// <summary>Gets the <see cref="JsonSerializerOptions"/> used for serializing outbox messages.</summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Serializes the specified value to JSON using <see cref="Options"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The JSON string representation of the value.</returns>
    public static string Serialize<T>(T value) where T : class
        => JsonSerializer.Serialize(value, Options);
}
