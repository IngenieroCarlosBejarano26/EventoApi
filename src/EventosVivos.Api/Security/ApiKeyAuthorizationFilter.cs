using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace EventosVivos.Api.Security;

public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";
    public string HeaderName { get; init; } = "X-API-KEY";
    public string Value { get; init; } = string.Empty;
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireApiKeyAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => true;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<ApiKeyAuthorizationFilter>(serviceProvider);
}

internal sealed class ApiKeyAuthorizationFilter(
    Microsoft.Extensions.Options.IOptions<ApiKeyOptions> options)
    : IAuthorizationFilter
{
    private readonly ApiKeyOptions _options = options.Value;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string headerName = _options.HeaderName;

        if (!context.HttpContext.Request.Headers.TryGetValue(headerName, out StringValues provided)
            || string.IsNullOrWhiteSpace(provided)
            || !string.Equals(provided, _options.Value, StringComparison.Ordinal))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = $"Se requiere una API Key vÃ¡lida en el header '{headerName}'."
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}
