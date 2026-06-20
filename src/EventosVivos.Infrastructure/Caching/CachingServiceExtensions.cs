using EventosVivos.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Infrastructure.Caching;

public static class CachingServiceExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        return services;
    }
}
