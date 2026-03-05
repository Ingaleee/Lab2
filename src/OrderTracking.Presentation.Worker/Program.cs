using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderTracking.Infrastructure.DI;
using OrderTracking.Infrastructure.Observability;
using OrderTracking.Presentation.Worker.Outbox;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

var otelSection = builder.Configuration.GetSection("OpenTelemetry");
var serviceName = otelSection["ServiceName"] ?? "order-tracking-worker";
var otlpEndpoint = otelSection.GetSection("Otlp")["Endpoint"] ?? "http://localhost:4317";
var enableOtlp = otelSection.GetSection("Exporters").GetValue<bool>("Otlp", true);
var enableConsole = otelSection.GetSection("Exporters").GetValue<bool>("Console", false);

var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

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

        if (enableOtlp)
        {
            metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
    });

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddKafkaProducer(builder.Configuration);

builder.Services.Configure<OutboxDispatcherOptions>(
    builder.Configuration.GetSection(OutboxDispatcherOptions.SectionName));

builder.Services.AddHostedService<OutboxDispatcherHostedService>();

var host = builder.Build();
await host.RunAsync();
