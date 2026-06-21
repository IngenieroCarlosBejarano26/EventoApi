using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace EventosVivos.Api.Security;

internal sealed class ApiKeyAuthorizationFilter(IOptions<ApiKeyOptions> options) : IAuthorizationFilter
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
                Detail = $"Se requiere una API Key válida en el header '{headerName}'."
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}
