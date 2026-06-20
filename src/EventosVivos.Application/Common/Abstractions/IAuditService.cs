namespace EventosVivos.Application.Common.Abstractions;

public interface IAuditService
{
    Task RecordAsync(string action, string details, CancellationToken cancellationToken = default);
}
