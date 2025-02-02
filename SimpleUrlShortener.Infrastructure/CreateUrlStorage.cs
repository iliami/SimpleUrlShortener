using SimpleUrlShortener.Domain;
using SimpleUrlShortener.Domain.CreateUrlUseCase;

namespace SimpleUrlShortener.Infrastructure;

public class CreateUrlStorage(IStringCacheStorage stringCacheStorage) : ICreateUrlStorage
{
    public async Task<Domain.Url> CreateUrl(UrlDto urlDto, CancellationToken ct = default)
    {
        await stringCacheStorage.Set(urlDto.UrlCode, urlDto.OriginalUrl, TimeSpan.FromHours(12), ct);

        return Domain.Url.Create(urlDto.OriginalUrl, urlDto.UrlCode);
    }
}