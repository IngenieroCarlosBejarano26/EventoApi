namespace EventosVivos.Application.Common.Abstractions;

/// <summary>
/// Puerto de cache. La implementación (IMemoryCache) vive en Infrastructure.
/// Permite invalidar por prefijo para mantener coherencia ante cambios.
/// </summary>
public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default);

    void Remove(string key);

    void RemoveByPrefix(string prefix);
}
