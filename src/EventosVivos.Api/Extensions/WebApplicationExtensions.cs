using EventosVivos.Api.Middleware;
using EventosVivos.Api.RealTime;
using EventosVivos.Infrastructure.Persistence;
using Scalar.AspNetCore;

namespace EventosVivos.Api.Extensions;

/// <summary>Configuración del pipeline HTTP y tareas de arranque, separadas del Program.</summary>
public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            // Documento OpenAPI nativo en /openapi/v1.json + UI interactiva (Scalar) en /scalar.
            app.MapOpenApi();
            app.MapScalarApiReference();
            // Comodidad: la raíz redirige a la UI de Scalar.
            app.MapGet("/", () => Results.Redirect("/scalar"));
        }

        app.UseCors(CorsPolicies.Frontend);
        app.UseRateLimiter();
        app.MapControllers();
        app.MapHub<EventsHub>("/hubs/events");

        return app;
    }

    /// <summary>Aplica migraciones y seed al arrancar (idempotente).</summary>
    public static Task InitializeDatabaseAsync(this WebApplication app) =>
        DatabaseInitializer.ApplyMigrationsAsync(app.Services);
}
