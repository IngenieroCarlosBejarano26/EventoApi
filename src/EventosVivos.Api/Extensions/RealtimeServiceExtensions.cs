using System.Text.Json;
using System.Text.Json.Serialization;
using EventosVivos.Api.RealTime;
using EventosVivos.Application.Common.Abstractions;

namespace EventosVivos.Api.Extensions;

/// <summary>Registra el transporte de tiempo real (SignalR) como adaptador del puerto IRealtimeNotifier.</summary>
public static class RealtimeServiceExtensions
{
    public static IServiceCollection AddRealtime(this IServiceCollection services)
    {
        services
            .AddSignalR()
            // Payloads consistentes con la API REST: camelCase y enums como texto.
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
        return services;
    }
}
