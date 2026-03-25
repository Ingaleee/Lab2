using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter.Prometheus;
using OrderTracking.Infrastructure.DI;
using OrderTracking.Infrastructure.Messaging.Kafka;
using OrderTracking.Infrastructure.Observability;
using OrderTracking.Infrastructure.Persistence;
using OrderTracking.Presentation.Api.DemoTraffic;
using OrderTracking.Presentation.Api.Messaging;
using OrderTracking.Presentation.Api.Middleware;
using OrderTracking.Presentation.Api.Realtime;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var otelSection = builder.Configuration.GetSection("OpenTelemetry");
var serviceName = otelSection["ServiceName"] ?? "order-tracking-api";
var otlpEndpoint = otelSection.GetSection("Otlp")["Endpoint"] ?? "http://localhost:4317";
var enableOtlp = otelSection.GetSection("Exporters").GetValue<bool>("Otlp", true);
var enableConsole = otelSection.GetSection("Exporters").GetValue<bool>("Console", false);

var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
        serviceName: serviceName,
        serviceVersion: serviceVersion,
        serviceInstanceId: Environment.MachineName,
        serviceNamespace: Telemetry.ServiceNamespace));
    if (enableOtlp)
    {
        logging.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    }
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(
        serviceName: serviceName,
        serviceVersion: serviceVersion,
        serviceInstanceId: Environment.MachineName,
        serviceNamespace: Telemetry.ServiceNamespace))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(Telemetry.ActivitySourceName)
            .AddAspNetCoreInstrumentation(opt =>
            {
                opt.RecordException = true;
            })
            .AddHttpClientInstrumentation(opt =>
            {
                opt.RecordException = true;
            })
            .AddEntityFrameworkCoreInstrumentation(opt =>
            {
                opt.SetDbStatementForText = true;
            });

        if (enableOtlp)
        {
            tracing.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
            });
        }

    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter(Telemetry.MeterName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();

        metrics.AddPrometheusExporter();

        if (enableOtlp)
        {
            metrics.AddOtlpExporter((o, reader) =>
            {
                o.Endpoint = new Uri(otlpEndpoint);
                reader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
            });
        }
    });

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton<OrderBookMetricsSnapshot>(_ =>
{
    var snapshot = new OrderBookMetricsSnapshot();
    OrderBookGaugeRegistration.Register(snapshot);
    return snapshot;
});
builder.Services.AddHostedService<OrderBookMetricsReporterHostedService>();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.SectionName));

builder.Services.AddHostedService<OrderStatusKafkaConsumerHostedService>();

if (builder.Configuration.GetValue("DemoTraffic:Enabled", false))
{
    builder.Services.AddHostedService<DemoTrafficHostedService>();
}

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.MapGet("/api-docs/openapi.yaml", async (HttpContext context) =>
{
    var yamlPath = Path.Combine(AppContext.BaseDirectory, "openapi.yaml");
    context.Response.ContentType = "application/x-yaml";
    await context.Response.SendFileAsync(yamlPath);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api-docs/openapi.yaml", "Order Tracking API v1");
    });
}

app.UseExceptionHandler("/error");
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseWebSockets();

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.MapHub<OrdersHub>("/hubs/orders")
    .RequireCors(policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());

app.Run();
