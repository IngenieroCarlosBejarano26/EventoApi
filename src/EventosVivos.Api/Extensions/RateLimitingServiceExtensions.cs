using System.Globalization;
using System.Threading.RateLimiting;
using EventosVivos.Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventosVivos.Api.Extensions;

public static class RateLimitingServiceExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(RateLimitingOptions.SectionName);

        services
            .AddOptions<RateLimitingOptions>()
            .Bind(section)
            .Validate(
                o => o.Global.PermitLimit > 0 && o.Global.WindowSeconds > 0
                     && o.Reservations.PermitLimit > 0 && o.Reservations.WindowSeconds > 0,
                "Los valores de 'RateLimiting' (PermitLimit y WindowSeconds) deben ser mayores que cero.")
            .ValidateOnStart();

        RateLimitingOptions options = section.Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (httpContext.Request.Path.StartsWithSegments("/hubs"))
                    return RateLimitPartition.GetNoLimiter("hub");

                string clientKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    clientKey, _ => ToFixedWindow(options.Global));
            });

            limiter.AddPolicy(RateLimitPolicies.Reservations, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => ToFixedWindow(options.Reservations)));

            limiter.OnRejected = OnRejectedAsync;
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions ToFixedWindow(RateLimitRule rule) => new()
    {
        PermitLimit = rule.PermitLimit,
        Window = TimeSpan.FromSeconds(rule.WindowSeconds),
        QueueLimit = 0
    };

    private static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        HttpResponse response = context.HttpContext.Response;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
            response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

        response.ContentType = "application/problem+json";
        await response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too Many Requests",
            Detail = "Has superado el límite de solicitudes permitido. Inténtalo de nuevo más tarde."
        }, cancellationToken);
    }
}
