using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderTracking.Application.Abstractions.Outbox;
using OrderTracking.Application.Abstractions.Persistence;
using OrderTracking.Application.Abstractions.Time;
using OrderTracking.Infrastructure.Outbox;
using OrderTracking.Infrastructure.Persistence;
using OrderTracking.Infrastructure.Persistence.Repositories;
using OrderTracking.Infrastructure.Time;

namespace OrderTracking.Infrastructure.DI;

/// <summary>
/// Provides extension methods for registering infrastructure services.
/// </summary>
public static class InfrastructureModule
{
    /// <summary>
    /// Adds infrastructure services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the 'Postgres' connection string is not configured.</exception>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(connectionString);
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxStore, EfOutboxStore>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
