namespace EventosVivos.Application.Common.Abstractions;

/// <summary>
/// Puerto de notificaciones. El adaptador actual simula el envío (logging); puede
/// reemplazarse por email/SMS o por un productor de mensajes sin tocar el dominio.
/// </summary>
public interface INotificationService
{
    Task NotifyAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
}
