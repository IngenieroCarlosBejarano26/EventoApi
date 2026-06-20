using System.Text.Json.Serialization;
using EventosVivos.Api.BackgroundServices;
using EventosVivos.Api.Security;
using EventosVivos.Application;
using EventosVivos.Infrastructure;

namespace EventosVivos.Api.Extensions;

/// <summary>
/// Punto único de composición de los servicios de la capa de presentación (API).
/// Cada preocupación se delega a un extension method especializado (SRP).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddOpenApiDocumentation();
        services.Configure<ApiKeyOptions>(configuration.GetSection(ApiKeyOptions.SectionName));

        // Capas internas (Application + Infrastructure) y tiempo real.
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddRealtime();

        services.AddHostedService<EventCompletionBackgroundService>();

        services.AddRateLimiting(configuration);
        services.AddFrontendCors(configuration);

        return services;
    }
}
