using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter.Prometheus;
using Microsoft.AspNetCore.Builder;
using OrderTracking.Infrastructure.DI;
using OrderTracking.Infrastructure.Observability;
using OrderTracking.Presentation.Worker.Outbox;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:9464");
builder.WebHost.Configure(app =>
{
    app.UseOpenTelemetryPrometheusScrapingEndpoint();
});

var otelSection = builder.Configuration.GetSection("OpenTelemetry");
var serviceName = otelSection["ServiceName"] ?? "order-tracking-worker";
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
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }

    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter(Telemetry.MeterName)
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

builder.Services.AddKafkaProducer(builder.Configuration);

builder.Services.Configure<OutboxDispatcherOptions>(
    builder.Configuration.GetSection(OutboxDispatcherOptions.SectionName));

builder.Services.AddHostedService<OutboxDispatcherHostedService>();

var host = builder.Build();
await host.RunAsync();
