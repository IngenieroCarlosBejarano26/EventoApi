using Microsoft.AspNetCore.Mvc.Filters;

namespace EventosVivos.Api.Security;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireApiKeyAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => true;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<ApiKeyAuthorizationFilter>(serviceProvider);
}
