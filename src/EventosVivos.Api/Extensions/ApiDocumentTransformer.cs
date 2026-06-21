using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EventosVivos.Api.Extensions;

internal sealed class ApiDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string ApiKeyScheme = "ApiKey";
    private const string IdempotencyScheme = "IdempotencyKey";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "EventosVivos API",
            Version = "v1",
            Description = """
                API REST del sistema de gestión de eventos y reservas.

                ## Autenticación
                Los endpoints administrativos requieren el header **X-API-KEY** (crear evento, confirmar pago).

                ## Idempotencia
                `POST /api/reservations` acepta **X-Idempotency-Key** para evitar reservas duplicadas en reintentos.

                ## Errores
                Las respuestas de error siguen [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807).
                El campo `errorCode` en `extensions` identifica el error de negocio.

                ## Tiempo real
                Hub SignalR en `/hubs/events` — eventos `EventCreated` y `EventUpdated`.
                """,
            Contact = new OpenApiContact
            {
                Name = "EventosVivos",
                Url = new Uri("https://github.com/IngenieroCarlosBejarano26/EventoApi")
            }
        };

        document.Tags = new HashSet<OpenApiTag>
        {
            new OpenApiTag
            {
                Name = "Events",
                Description = "Alta, consulta filtrada y reportes de ocupación de eventos."
            },
            new OpenApiTag
            {
                Name = "Reservations",
                Description = "Reserva de entradas, confirmación de pago y cancelación."
            },
            new OpenApiTag
            {
                Name = "Venues",
                Description = "Catálogo de sedes disponibles."
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[ApiKeyScheme] = new OpenApiSecurityScheme
        {
            Name = "X-API-KEY",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "Clave de API para operaciones administrativas."
        };

        document.Components.SecuritySchemes[IdempotencyScheme] = new OpenApiSecurityScheme
        {
            Name = "X-Idempotency-Key",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "Clave única por intento de reserva. Reintentos con la misma clave no duplican la operación."
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(ApiKeyScheme, document)] = []
        });

        return Task.CompletedTask;
    }
}
