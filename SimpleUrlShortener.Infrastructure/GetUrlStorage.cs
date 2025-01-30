using SimpleUrlShortener.Domain.GetUrlUseCase;

namespace SimpleUrlShortener.Infrastructure;

public class GetUrlStorage(IReadonlyCache readonlyCache) : IGetUrlStorage
{
    public Task<string?> GetUrl(string urlCode, CancellationToken ct = default)
        => readonlyCache.Get<string>(urlCode, ct);
}