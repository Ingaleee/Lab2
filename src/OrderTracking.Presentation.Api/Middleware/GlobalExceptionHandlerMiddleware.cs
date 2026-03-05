using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderTracking.Application.Common.Errors;

namespace OrderTracking.Presentation.Api.Middleware;

/// <summary>
/// Global exception handler middleware that converts exceptions to <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>.
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate.</param>
    /// <param name="logger">The logger.</param>
    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException notFound => ((int)HttpStatusCode.NotFound, "Not Found", notFound.Message),
            ConflictException conflict => ((int)HttpStatusCode.Conflict, "Conflict", conflict.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error", "An error occurred while processing your request.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title,
            status = statusCode,
            detail,
            instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
