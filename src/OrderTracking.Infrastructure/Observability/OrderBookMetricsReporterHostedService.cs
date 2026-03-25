using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderTracking.Domain.Orders;
using OrderTracking.Infrastructure.Persistence;

namespace OrderTracking.Infrastructure.Observability;

public sealed class OrderBookMetricsReporterHostedService : BackgroundService
{
    private static readonly TimeSpan Tick = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan NewQueueSla = TimeSpan.FromHours(24);
    private static readonly TimeSpan InProgressStallSla = TimeSpan.FromHours(72);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OrderBookMetricsSnapshot _snapshot;
    private readonly ILogger<OrderBookMetricsReporterHostedService> _logger;

    public OrderBookMetricsReporterHostedService(
        IServiceScopeFactory scopeFactory,
        OrderBookMetricsSnapshot snapshot,
        ILogger<OrderBookMetricsReporterHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _snapshot = snapshot;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Order book metrics refresh failed");
            }

            try
            {
                await Task.Delay(Tick, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }
    }

    private async Task RefreshAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTimeOffset.UtcNow;

        var rows = await db.Orders.AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var dict = new Dictionary<OrderStatus, long>(4);
        foreach (var row in rows)
        {
            dict[row.Key] = row.Count;
        }

        var overdueNew = await db.Orders.AsNoTracking()
            .CountAsync(o => o.Status == OrderStatus.New && o.CreatedAt < now - NewQueueSla, ct);

        var staleInProgress = await db.Orders.AsNoTracking()
            .CountAsync(o => o.Status == OrderStatus.InProgress && o.UpdatedAt < now - InProgressStallSla, ct);

        _snapshot.Update(dict, overdueNew, staleInProgress);
    }
}
