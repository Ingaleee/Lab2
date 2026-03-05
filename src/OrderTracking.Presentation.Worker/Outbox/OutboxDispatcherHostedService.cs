using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderTracking.Infrastructure.Messaging.Kafka;
using OrderTracking.Infrastructure.Observability;
using OrderTracking.Infrastructure.Outbox;
using OrderTracking.Infrastructure.Persistence;

namespace OrderTracking.Presentation.Worker.Outbox;

/// <summary>
/// Background worker that publishes <see cref="OutboxMessage"/> messages to Kafka reliably.
/// </summary>
public sealed class OutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;
    private readonly OutboxDispatcherOptions _options;
    private readonly KafkaOptions _kafkaOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxDispatcherHostedService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="options">The outbox dispatcher options.</param>
    /// <param name="kafkaOptions">The Kafka options.</param>
    /// <param name="logger">The logger.</param>
    public OutboxDispatcherHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxDispatcherOptions> options,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<OutboxDispatcherHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _kafkaOptions = kafkaOptions.Value;
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox dispatcher started. BatchSize={BatchSize}, Poll={Poll}s",
            _options.BatchSize, _options.PollIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await DispatchOnce(stoppingToken);

                if (processed == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox dispatcher loop failed");
                await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }
        }

        _logger.LogInformation("Outbox dispatcher stopped.");
    }

    private async Task<int> DispatchOnce(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var producer = scope.ServiceProvider.GetRequiredService<IKafkaProducer>();

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var pendingStatus = (int)OutboxMessageStatus.Pending;

        var batch = await db.OutboxMessages
            .FromSqlInterpolated($@"
                SELECT *
                FROM outbox_messages
                WHERE status = {pendingStatus}
                ORDER BY occurred_at
                LIMIT {_options.BatchSize}
                FOR UPDATE SKIP LOCKED
            ")
            .ToListAsync(ct);

        if (batch.Count == 0)
        {
            await tx.RollbackAsync(ct);
            return 0;
        }

        _logger.LogInformation("Outbox batch selected: {Count}", batch.Count);

        foreach (var msg in batch)
        {
            if (msg.Status != OutboxMessageStatus.Pending)
                continue;

            using var dispatchActivity = Telemetry.ActivitySource.StartActivity(
                "Outbox.Dispatch",
                ActivityKind.Internal);

            dispatchActivity?.SetTag("outbox.message_id", msg.Id.ToString());
            dispatchActivity?.SetTag("integration.event_type", msg.Type);
            dispatchActivity?.SetTag("kafka.topic", _kafkaOptions.OrderStatusTopic);
            dispatchActivity?.SetTag("outbox.attempt", msg.Attempts);

            try
            {
                var key = BuildMessageKey(msg);
                var headers = new Dictionary<string, string>
                {
                    [KafkaHeaders.EventId] = msg.Id.ToString(),
                    [KafkaHeaders.EventType] = msg.Type,
                    [KafkaHeaders.OccurredAt] = msg.OccurredAt.ToString("O")
                };

                using var produceActivity = Telemetry.ActivitySource.StartActivity(
                    "Kafka.Produce",
                    ActivityKind.Producer);

                produceActivity?.SetTag("messaging.system", "kafka");
                produceActivity?.SetTag("messaging.destination", _kafkaOptions.OrderStatusTopic);
                produceActivity?.SetTag("messaging.operation", "send");
                produceActivity?.SetTag("messaging.message_id", msg.Id.ToString());

                try
                {
                    using var doc = JsonDocument.Parse(msg.Payload);
                    if (doc.RootElement.TryGetProperty("eventId", out var eventId))
                    {
                        var eventIdStr = eventId.GetString();
                        if (!string.IsNullOrWhiteSpace(eventIdStr))
                        {
                            produceActivity?.SetTag("integration.event_id", eventIdStr);
                        }
                    }
                }
                catch
                {
                }

                await producer.PublishAsync(
                    topic: _kafkaOptions.OrderStatusTopic,
                    key: key,
                    payload: msg.Payload,
                    headers: headers,
                    cancellationToken: ct);

                msg.MarkProcessed(DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                dispatchActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                dispatchActivity?.SetTag("error", true);
                dispatchActivity?.SetTag("error.message", ex.Message);

                var error = ex.Message;
                _logger.LogWarning(ex, "Failed to publish outbox message. Id={Id}, Attempts={Attempts}",
                    msg.Id, msg.Attempts);

                if (msg.Attempts + 1 >= _options.MaxAttempts)
                {
                    msg.MarkPoisoned(error);
                    _logger.LogError("Outbox message poisoned. Id={Id}", msg.Id);
                }
                else
                {
                    msg.MarkFailed(error);
                }
            }
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var processedCount = batch.Count(x => x.Status == OutboxMessageStatus.Processed);
        _logger.LogInformation("Outbox batch processed. Success={Success}, Total={Total}",
            processedCount, batch.Count);

        return batch.Count;
    }

    private static string BuildMessageKey(OutboxMessage msg)
    {
        try
        {
            using var doc = JsonDocument.Parse(msg.Payload);
            if (doc.RootElement.TryGetProperty("orderId", out var orderId))
            {
                var s = orderId.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    return s!;
            }
        }
        catch
        {
        }

        return msg.Id.ToString();
    }
}
