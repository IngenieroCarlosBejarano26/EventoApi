using EventosVivos.Application.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Infrastructure.Services;

/// <summary>Registro de los servicios de soporte (tiempo, auditoría y notificaciones simuladas).</summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}
