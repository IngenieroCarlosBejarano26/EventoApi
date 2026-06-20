using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace EventosVivos.Api.Idempotency;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => true;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<IdempotencyFilter>(serviceProvider);
}

internal sealed class IdempotencyFilter(IMemoryCache cache) : IAsyncActionFilter
{
    public const string HeaderName = "X-Idempotency-Key";
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    private sealed record CachedResponse(int StatusCode, object? Body);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out StringValues keyValues)
            || string.IsNullOrWhiteSpace(keyValues))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "MissingIdempotencyKey",
                Detail = $"Se requiere el header '{HeaderName}' para esta operaciÃ³n."
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            return;
        }

        string cacheKey = $"idem:{keyValues}";

        if (cache.TryGetValue(cacheKey, out CachedResponse? cached) && cached is not null)
        {
            context.Result = new ObjectResult(cached.Body) { StatusCode = cached.StatusCode };
            return;
        }

        ActionExecutedContext executed = await next();

        if (executed.Result is ObjectResult { StatusCode: >= 200 and < 300 } result)
        {
            cache.Set(cacheKey, new CachedResponse(result.StatusCode ?? 200, result.Value), Ttl);
        }
    }
}
