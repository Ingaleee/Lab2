using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderTracking.Infrastructure.Observability;
using OrderTracking.Infrastructure.Persistence;
using OrderTracking.Presentation.Api.Messaging;

namespace OrderTracking.Presentation.Api.IntegrationTests;

/// <summary>
/// Boots the API with an in-memory database (no Postgres/Kafka) for smoke tests.
/// </summary>
public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");
        builder.UseSetting("OpenTelemetry:Exporters:Otlp", "false");
        builder.UseSetting("ConnectionStrings:Postgres", "Host=127.0.0.1;Database=test_only_unused;Username=u;Password=p");

        builder.ConfigureTestServices(services =>
        {
            foreach (var d in services.Where(x =>
                         x.ImplementationType == typeof(OrderStatusKafkaConsumerHostedService)
                         || x.ImplementationType == typeof(OrderBookMetricsReporterHostedService)).ToList())
            {
                services.Remove(d);
            }

            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("OrderTracking_IntegrationTests"));
        });
    }
}
