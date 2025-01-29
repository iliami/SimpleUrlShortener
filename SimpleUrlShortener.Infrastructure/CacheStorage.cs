using Microsoft.Extensions.Caching.Memory;

namespace SimpleUrlShortener.Infrastructure;

public class CacheStorage(IMemoryCache memoryCache) : ICacheStorage
{
    public Task<T?> Get<T>(string key, CancellationToken ct = default)
        => Task.FromResult(memoryCache.Get<T>(key));

    public Task Set<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
    {
        memoryCache.Set(key, value, expiration);
        return Task.CompletedTask;
    }
}