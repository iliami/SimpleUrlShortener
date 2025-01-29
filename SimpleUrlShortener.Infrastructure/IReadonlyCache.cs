namespace SimpleUrlShortener.Infrastructure;

public interface IReadonlyCache
{
    public Task<T?> Get<T>(string key, CancellationToken ct = default);
}