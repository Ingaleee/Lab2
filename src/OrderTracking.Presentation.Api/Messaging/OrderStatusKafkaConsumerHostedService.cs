using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderTracking.Contracts.IntegrationEvents;
using OrderTracking.Infrastructure.Idempotency;
using OrderTracking.Infrastructure.Messaging.Kafka;
using OrderTracking.Infrastructure.Observability;
using OrderTracking.Infrastructure.Persistence;
using OrderTracking.Presentation.Api.Realtime;

namespace OrderTracking.Presentation.Api.Messaging;

/// <summary>
/// Consumes order status events from Kafka and broadcasts them to SignalR clients.
/// </summary>
public sealed class OrderStatusKafkaConsumerHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<OrdersHub> _hub;
    private readonly ILogger<OrderStatusKafkaConsumerHostedService> _logger;
    private readonly KafkaOptions _kafka;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderStatusKafkaConsumerHostedService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="hub">The SignalR hub context.</param>
    /// <param name="kafkaOptions">The Kafka options.</param>
    /// <param name="logger">The logger.</param>
    public OrderStatusKafkaConsumerHostedService(
        IServiceScopeFactory scopeFactory,
        IHubContext<OrdersHub> hub,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<OrderStatusKafkaConsumerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _hub = hub;
        _logger = logger;
        _kafka = kafkaOptions.Value;
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafka.BootstrapServers,
            ClientId = string.IsNullOrWhiteSpace(_kafka.ClientId) ? "order-tracking-api" : _kafka.ClientId,
            GroupId = _kafka.ConsumerGroupId,
            EnableAutoCommit = _kafka.EnableAutoCommit,
            AutoOffsetReset = ParseAutoOffsetReset(_kafka.AutoOffsetReset),
            EnablePartitionEof = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
            .Build();

        consumer.Subscribe(_kafka.OrderStatusTopic);

        _logger.LogInformation("Kafka consumer started. Topic={Topic}, Group={Group}",
            _kafka.OrderStatusTopic, _kafka.ConsumerGroupId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cr = consumer.Consume(stoppingToken);
                if (cr?.Message?.Value is null)
                    continue;

                var evt = Deserialize(cr.Message.Value);
                if (evt is null || evt.EventId == Guid.Empty || evt.OrderId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid event payload. Skipping. Offset={Offset}", cr.Offset.Value);
                    consumer.Commit(cr);
                    continue;
                }

                using var consumeActivity = Telemetry.ActivitySource.StartActivity(
                    "Kafka.Consume",
                    ActivityKind.Consumer);

                consumeActivity?.SetTag("messaging.system", "kafka");
                consumeActivity?.SetTag("messaging.destination", _kafka.OrderStatusTopic);
                consumeActivity?.SetTag("messaging.operation", "receive");
                consumeActivity?.SetTag("integration.event_id", evt.EventId.ToString());
                consumeActivity?.SetTag("order.id", evt.OrderId.ToString());
                consumeActivity?.SetTag("order.old_status", evt.OldStatus);
                consumeActivity?.SetTag("order.new_status", evt.NewStatus);

                if (await IsAlreadyProcessed(evt.EventId, stoppingToken))
                {
                    _logger.LogInformation("Event already processed. EventId={EventId}", evt.EventId);
                    consumer.Commit(cr);
                    continue;
                }

                using var broadcastActivity = Telemetry.ActivitySource.StartActivity(
                    "SignalR.Broadcast",
                    ActivityKind.Producer);

                broadcastActivity?.SetTag("signalr.group.list", "orders:list");
                broadcastActivity?.SetTag("signalr.group.order", $"order:{evt.OrderId}");
                broadcastActivity?.SetTag("signalr.event", "orderStatusChanged");

                await Broadcast(evt, stoppingToken);

                await MarkProcessed(evt.EventId, stoppingToken);

                consumer.Commit(cr);
            }
                catch (ConsumeException ex)
                {
                    if (ex.Error.Code == Confluent.Kafka.ErrorCode.UnknownTopicOrPart)
                    {
                        _logger.LogWarning("Kafka topic not found. Waiting for topic creation. Topic={Topic}", _kafka.OrderStatusTopic);
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    else
                    {
                        _logger.LogError(ex, "Kafka consume exception");
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consumer loop failed");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        consumer.Close();
        _logger.LogInformation("Kafka consumer stopped.");
    }

    private static AutoOffsetReset ParseAutoOffsetReset(string value)
        => string.Equals(value, "Latest", StringComparison.OrdinalIgnoreCase)
            ? AutoOffsetReset.Latest
            : AutoOffsetReset.Earliest;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static OrderStatusChangedIntegrationEventV1? Deserialize(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<OrderStatusChangedIntegrationEventV1>(payload, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private async Task Broadcast(OrderStatusChangedIntegrationEventV1 evt, CancellationToken ct)
    {
        await _hub.Clients.Group(OrdersHubGroups.OrdersList)
            .SendAsync(HubEvents.OrderStatusChanged, evt, ct);

        await _hub.Clients.Group(OrdersHubGroups.Order(evt.OrderId))
            .SendAsync(HubEvents.OrderStatusChanged, evt, ct);

        _logger.LogInformation(
            "Broadcasted status update. EventId={EventId}, OrderId={OrderId}, {Old}->{New}",
            evt.EventId, evt.OrderId, evt.OldStatus, evt.NewStatus);
    }

    private async Task<bool> IsAlreadyProcessed(Guid eventId, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.ProcessedEvents.AnyAsync(x => x.EventId == eventId, ct);
    }

    private async Task MarkProcessed(Guid eventId, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.ProcessedEvents.Add(new ProcessedEvent(eventId, DateTimeOffset.UtcNow));

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
        }
    }
}
