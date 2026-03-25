using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

namespace OrderTracking.Presentation.Api.DemoTraffic;

/// <summary>HTTP-прогон сценариев заказов при старте (для наполнения БД, метрик Prometheus и логов Loki без ручного curl).</summary>
public sealed class DemoTrafficHostedService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IHostApplicationLifetime _hostLifetime;
    private readonly ILogger<DemoTrafficHostedService> _logger;

    public DemoTrafficHostedService(
        IConfiguration configuration,
        IHostApplicationLifetime hostLifetime,
        ILogger<DemoTrafficHostedService> logger)
    {
        _configuration = configuration;
        _hostLifetime = hostLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue("DemoTraffic:Enabled", false))
        {
            return;
        }

        var baseUrl = _configuration["DemoTraffic:BaseUrl"] ?? "http://127.0.0.1:8080";
        var rounds = _configuration.GetValue("DemoTraffic:Rounds", 2);
        var delayMs = _configuration.GetValue("DemoTraffic:DelayMs", 4000);

        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _hostLifetime.ApplicationStarted.Register(started.SetResult);
        await started.Task.WaitAsync(stoppingToken);
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        using var client = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        client.Timeout = TimeSpan.FromMinutes(3);

        try
        {
            for (var r = 0; r < rounds && !stoppingToken.IsCancellationRequested; r++)
            {
                var created = new List<Guid>();
                for (var i = 0; i < 10; i++)
                {
                    var num = $"AUTO-DEMO-r{r}-n{i}-{Random.Shared.Next(100000, 999999)}";
                    var createdDto = await client.PostAsJsonAsync(
                        "api/orders",
                        new CreateOrderDto(num, $"Auto demo traffic ({num})"),
                        stoppingToken);
                    createdDto.EnsureSuccessStatusCode();
                    var body = await createdDto.Content.ReadFromJsonAsync<OrderResponseDto>(cancellationToken: stoppingToken);
                    if (body?.Id is { } id)
                    {
                        created.Add(id);
                    }
                }

                for (var k = 0; k < 5; k++)
                {
                    using var list = await client.GetAsync("api/orders", stoppingToken);
                    list.EnsureSuccessStatusCode();
                }

                foreach (var id in created)
                {
                    using var one = await client.GetAsync($"api/orders/{id:D}", stoppingToken);
                    one.EnsureSuccessStatusCode();
                }

                if (created.Count < 8)
                {
                    continue;
                }

                await RunTransitions(client, created, delayMs, stoppingToken);

                for (var k = 0; k < 8; k++)
                {
                    using var list = await client.GetAsync("api/orders", stoppingToken);
                    list.EnsureSuccessStatusCode();
                }
            }

            _logger.LogInformation("DemoTraffic: прогон завершён ({Rounds} раунд(ов)); обновите Grafana (Last 15m).", rounds);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "DemoTraffic: ошибка прогона (метрики могут остаться нулевыми).");
        }
    }

    private static async Task RunTransitions(HttpClient client, List<Guid> ids, int delayMs, CancellationToken ct)
    {
        var a = ids[0];
        var b = ids[1];
        var c = ids[2];
        var d = ids[3];
        var e = ids[4];
        var f = ids[5];
        var g = ids[6];
        var h = ids[7];

        async Task Patch(Guid id, string status)
        {
            using var res = await client.PatchAsJsonAsync(
                $"api/orders/{id:D}/status",
                new UpdateStatusDto(status),
                ct);
            res.EnsureSuccessStatusCode();
        }

        async Task Wait()
        {
            await Task.Delay(delayMs, ct);
        }

        await Patch(a, "InProgress");
        await Wait();
        await Patch(a, "Delivered");
        await Wait();

        await Patch(b, "InProgress");
        await Wait();
        await Patch(b, "Cancelled");
        await Wait();

        await Patch(c, "Cancelled");
        await Wait();

        await Patch(d, "InProgress");
        await Wait();
        await Patch(d, "Delivered");
        await Wait();

        await Patch(e, "InProgress");
        await Wait();
        await Patch(e, "Delivered");
        await Wait();

        await Patch(f, "Cancelled");
        await Wait();

        await Patch(g, "InProgress");
        await Wait();
        await Patch(g, "Cancelled");
        await Wait();

        await Patch(h, "InProgress");
        await Wait();
        await Patch(h, "Delivered");
        await Wait();
    }

    private sealed record CreateOrderDto(string OrderNumber, string Description);

    private sealed record UpdateStatusDto(string Status);

    private sealed record OrderResponseDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }
    }
}
