using Microsoft.Extensions.Caching.Memory;

namespace SimpleUrlShortener.Infrastructure;

public interface IStringCacheStorage
{
    public Task<string?> Get(string key, CancellationToken ct = default);
    public Task Set(string key, string value, TimeSpan expiration, CancellationToken ct = default);
}

public class StringCacheStorage(IMemoryCache memoryCache) : IStringCacheStorage
{
    public Task<string?> Get(string key, CancellationToken ct = default)
        => Task.FromResult(memoryCache.Get<string>(key));

    public Task Set(string key, string value, TimeSpan expiration, CancellationToken ct = default)
        => Task.Run(() => memoryCache.Set(key, value, expiration), ct);
}