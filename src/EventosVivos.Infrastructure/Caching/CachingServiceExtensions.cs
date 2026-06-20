using EventosVivos.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Infrastructure.Caching;

/// <summary>Registro del cache en memoria y su adaptador del puerto ICacheService.</summary>
public static class CachingServiceExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        return services;
    }
}
