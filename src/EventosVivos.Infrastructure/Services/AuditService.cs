using EventosVivos.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace EventosVivos.Infrastructure.Services;

/// <summary>
/// Adaptador de auditoría simulado mediante logging estructurado.
/// Reemplazable por persistencia en BD o publicación a un bus de mensajes.
/// </summary>
internal sealed class AuditService(ILogger<AuditService> logger) : IAuditService
{
    public Task RecordAsync(string action, string details, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[AUDIT] {Action} :: {Details}", action, details);
        return Task.CompletedTask;
    }
}
