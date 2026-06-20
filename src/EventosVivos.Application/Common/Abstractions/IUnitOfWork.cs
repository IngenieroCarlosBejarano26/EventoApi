namespace EventosVivos.Application.Common.Abstractions;

/// <summary>
/// Unit of Work: confirma de forma atómica los cambios del agregado y dispara
/// el despacho de los eventos de dominio dentro de la misma transacción lógica.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
