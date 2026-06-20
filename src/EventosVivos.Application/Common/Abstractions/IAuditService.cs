namespace EventosVivos.Application.Common.Abstractions;

/// <summary>
/// Puerto de auditoría. El adaptador actual escribe a log estructurado; en producción
/// podría persistir en una tabla de auditoría o publicar a un bus (RabbitMQ / Azure Service Bus).
/// </summary>
public interface IAuditService
{
    Task RecordAsync(string action, string details, CancellationToken cancellationToken = default);
}
