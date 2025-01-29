namespace SimpleUrlShortener.Infrastructure;

public interface ICacheStorage : IReadonlyCache
{
    public Task Set<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default);
}