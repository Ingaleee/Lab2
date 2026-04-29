namespace OrderTracking.Presentation.Api.IntegrationTests;

public sealed class ApiSmokeTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_returns_success()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task OpenApi_yaml_is_served()
    {
        var response = await _client.GetAsync("/api-docs/openapi.yaml");
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/x-yaml", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Security_headers_are_on_health_response()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        Assert.True(response.Headers.TryGetValues("X-Content-Type-Options", out var contentTypeOpts), "Expected X-Content-Type-Options");
        Assert.Contains("nosniff", contentTypeOpts.First(), StringComparison.OrdinalIgnoreCase);

        Assert.True(response.Headers.TryGetValues("X-Frame-Options", out var frameOpts), "Expected X-Frame-Options");
        Assert.Contains("DENY", frameOpts.First(), StringComparison.OrdinalIgnoreCase);

        Assert.True(response.Headers.TryGetValues("Referrer-Policy", out var referrerPolicy), "Expected Referrer-Policy");
        Assert.Contains("strict-origin-when-cross-origin", referrerPolicy.First(), StringComparison.OrdinalIgnoreCase);

        Assert.True(response.Headers.TryGetValues("Permissions-Policy", out var permissionsPolicy), "Expected Permissions-Policy");
        Assert.NotNull(permissionsPolicy);
        Assert.Contains("camera=()", permissionsPolicy.First(), StringComparison.Ordinal);
    }
}
