using System.Diagnostics;

namespace EventosVivos.Api.Middleware;

/// <summary>Logging estructurado de cada request/response con su duración.</summary>
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        logger.LogInformation("HTTP {Method} {Path} iniciada",
            context.Request.Method, context.Request.Path);

        await next(context);

        stopwatch.Stop();
        logger.LogInformation("HTTP {Method} {Path} respondió {StatusCode} en {ElapsedMs} ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
    }
}
