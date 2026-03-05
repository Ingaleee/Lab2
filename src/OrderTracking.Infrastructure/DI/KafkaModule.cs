using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderTracking.Infrastructure.Messaging.Kafka;

namespace OrderTracking.Infrastructure.DI;

/// <summary>
/// Provides extension methods for registering Kafka services.
/// </summary>
public static class KafkaModule
{
    /// <summary>
    /// Adds Kafka producer services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<IKafkaProducer, KafkaProducer>();
        return services;
    }
}
