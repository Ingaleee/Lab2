namespace OrderTracking.Presentation.Api.Middleware;

/// <summary>
/// Adds baseline HTTP security headers for browser clients (API is consumed by SPA).
/// Uses response OnStarting so headers are still set when the pipeline handles the request without a normal return.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            var ctx = (HttpContext)state!;
            AddHeaders(ctx.Response.Headers);
            return Task.CompletedTask;
        }, context);

        return _next(context);
    }

    private static void AddHeaders(IHeaderDictionary headers)
    {
        AppendIfMissing(headers, "X-Content-Type-Options", "nosniff");
        AppendIfMissing(headers, "X-Frame-Options", "DENY");
        AppendIfMissing(headers, "Referrer-Policy", "strict-origin-when-cross-origin");
        AppendIfMissing(
            headers,
            "Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
    }

    private static void AppendIfMissing(IHeaderDictionary headers, string name, string value)
    {
        if (!headers.ContainsKey(name))
            headers.Append(name, value);
    }
}
