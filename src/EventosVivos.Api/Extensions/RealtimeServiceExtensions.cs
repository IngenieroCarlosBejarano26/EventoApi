using System.Text.Json;
using System.Text.Json.Serialization;
using EventosVivos.Api.RealTime;
using EventosVivos.Application.Common.Abstractions;

namespace EventosVivos.Api.Extensions;

public static class RealtimeServiceExtensions
{
    public static IServiceCollection AddRealtime(this IServiceCollection services)
    {
        services
            .AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
        return services;
    }
}
