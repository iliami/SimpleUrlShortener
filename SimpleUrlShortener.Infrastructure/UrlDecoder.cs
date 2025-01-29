using SimpleUrlShortener.Domain;

namespace SimpleUrlShortener.Infrastructure;

public class UrlDecoder(IReadonlyCache readonlyCache) : IUrlDecoder
{
    public Task<string?> Decode(string code)
        => readonlyCache.Get<string?>(code);
}