using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EventosVivos.Api.Extensions;

/// <summary>
/// Documentación OpenAPI nativa de ASP.NET Core (.NET 10). Sustituye a Swashbuckle/Swagger:
/// el documento se genera desde los metadatos del framework y se enriquece con transformers.
/// </summary>
public static class OpenApiServiceExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services) =>
        services.AddOpenApi(options => options.AddDocumentTransformer<ApiKeyDocumentTransformer>());
}

/// <summary>Añade la metadata del documento y el esquema de seguridad por API Key (X-API-KEY).</summary>
internal sealed class ApiKeyDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string ApiKeyScheme = "ApiKey";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "EventosVivos API",
            Version = "v1",
            Description = "Núcleo del sistema de gestión de eventos y reservas."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[ApiKeyScheme] = new OpenApiSecurityScheme
        {
            Name = "X-API-KEY",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "API Key para endpoints administrativos."
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(ApiKeyScheme, document)] = []
        });

        return Task.CompletedTask;
    }
}
