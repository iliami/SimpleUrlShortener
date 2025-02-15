using Microsoft.Extensions.Caching.Memory;

namespace SimpleUrlShortener.UrlShortener.Infrastructure;

public interface ICacheStorage
{
    public Task<T?> Get<T>(string key, CancellationToken ct = default);
    public Task Set<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default);
}

public class CacheStorage(IMemoryCache memoryCache) : ICacheStorage
{
    public Task<T?> Get<T>(string key, CancellationToken ct = default)
        => Task.FromResult(memoryCache.Get<T?>(key));

    public Task Set<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
        => Task.Run(() => memoryCache.Set(key, value, expiration), ct);
}