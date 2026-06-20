using EventosVivos.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Infrastructure.Services;

/// <summary>
/// Adaptador de notificaciones simulado (logging). Reemplazable por email/SMS o un
/// productor de mensajes (RabbitMQ / Azure Service Bus) sin afectar al dominio.
/// </summary>
internal sealed class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public Task NotifyAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[NOTIFICATION] Para: {Recipient} | Asunto: {Subject} | Mensaje: {Body}",
            string.IsNullOrWhiteSpace(recipient) ? "(n/a)" : recipient, subject, body);
        return Task.CompletedTask;
    }
}
