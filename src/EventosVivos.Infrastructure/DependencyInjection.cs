using EventosVivos.Infrastructure.Caching;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddPersistence(configuration)
            .AddCaching()
            .AddInfrastructureServices();
}
