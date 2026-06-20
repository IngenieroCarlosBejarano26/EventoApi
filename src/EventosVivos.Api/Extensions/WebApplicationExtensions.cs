using EventosVivos.Api.Middleware;
using EventosVivos.Api.RealTime;
using EventosVivos.Infrastructure.Persistence;
using Scalar.AspNetCore;

namespace EventosVivos.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.MapGet("/", () => Results.Redirect("/scalar"));
        }

        app.UseCors(CorsPolicies.Frontend);
        app.UseRateLimiter();
        app.MapControllers();
        app.MapHub<EventsHub>("/hubs/events");

        return app;
    }

    public static Task InitializeDatabaseAsync(this WebApplication app) =>
        DatabaseInitializer.ApplyMigrationsAsync(app.Services);
}
