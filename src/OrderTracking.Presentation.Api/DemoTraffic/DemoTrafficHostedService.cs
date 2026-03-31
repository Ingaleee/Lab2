using System.Net.Http.Json;
using System.Text.Json;

namespace OrderTracking.Presentation.Api.DemoTraffic;

file static class DemoTrafficJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}

/// <summary>
/// Periodically calls the local HTTP API to generate traces, metrics, and logs for demos.
/// </summary>
internal sealed class DemoTrafficHostedService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DemoTrafficHostedService> _logger;

    public DemoTrafficHostedService(
        IConfiguration configuration,
        ILogger<DemoTrafficHostedService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var baseUrl = (_configuration["DemoTraffic:BaseUrl"] ?? "http://127.0.0.1:8080").TrimEnd('/');
        var rounds = _configuration.GetValue("DemoTraffic:Rounds", 2);
        var delayMs = _configuration.GetValue("DemoTraffic:DelayMs", 4000);

        _logger.LogInformation(
            "DemoTraffic: BaseUrl={BaseUrl}, Rounds={Rounds}, DelayMs={DelayMs}",
            baseUrl,
            rounds,
            delayMs);

        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);

        using var client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl + "/"),
            Timeout = TimeSpan.FromSeconds(60)
        };

        for (var r = 0; r < rounds && !stoppingToken.IsCancellationRequested; r++)
        {
            try
            {
                await RunRoundAsync(client, r, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "DemoTraffic round {Round} failed", r);
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(delayMs, stoppingToken).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("DemoTraffic finished after {Rounds} rounds", rounds);
    }

    private static async Task RunRoundAsync(HttpClient client, int round, CancellationToken ct)
    {
        var suffix = $"{DateTimeOffset.UtcNow:HHmmssfff}-r{round}";
        using var createRes = await client.PostAsJsonAsync(
                "api/orders",
                new { orderNumber = $"DEMO-{suffix}", description = "Demo traffic" },
                DemoTrafficJson.Options,
                ct)
            .ConfigureAwait(false);
        createRes.EnsureSuccessStatusCode();
        var createJson = await createRes.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        using var doc = JsonDocument.Parse(createJson);
        var id = doc.RootElement.GetProperty("id").GetGuid();

        await client.GetAsync("api/orders", ct).ConfigureAwait(false);
        await client.GetAsync($"api/orders/{id}", ct).ConfigureAwait(false);

        foreach (var status in new[] { "InProgress", "Delivered" })
        {
            using var patchRes = await client.PatchAsJsonAsync(
                    $"api/orders/{id}/status",
                    new { status },
                    DemoTrafficJson.Options,
                    ct)
                .ConfigureAwait(false);
            patchRes.EnsureSuccessStatusCode();
        }
    }
}
