using System.Text.Json.Serialization;
using EventosVivos.Api.BackgroundServices;
using EventosVivos.Api.Security;
using EventosVivos.Application;
using EventosVivos.Infrastructure;

namespace EventosVivos.Api.Extensions;

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

        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddRealtime();

        services.AddHostedService<EventCompletionBackgroundService>();

        services.AddRateLimiting(configuration);
        services.AddFrontendCors(configuration);

        return services;
    }
}
