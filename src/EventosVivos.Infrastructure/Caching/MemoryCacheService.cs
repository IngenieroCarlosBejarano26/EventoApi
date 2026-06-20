using System.Collections.Concurrent;
using EventosVivos.Application.Common.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace EventosVivos.Infrastructure.Caching;

internal sealed class MemoryCacheService(IMemoryCache cache) : ICacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);
    private static readonly ConcurrentDictionary<string, byte> Keys = new();

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        T value = await factory(cancellationToken);

        cache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? DefaultTtl
        });
        Keys.TryAdd(key, 0);

        return value;
    }

    public void Remove(string key)
    {
        cache.Remove(key);
        Keys.TryRemove(key, out _);
    }

    public void RemoveByPrefix(string prefix)
    {
        foreach (string key in Keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)))
            Remove(key);
    }
}
