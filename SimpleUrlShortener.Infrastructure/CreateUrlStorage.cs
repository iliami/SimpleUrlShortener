using SimpleUrlShortener.Domain;
using SimpleUrlShortener.Domain.CreateUrlUseCase;

namespace SimpleUrlShortener.Infrastructure;

public class CreateUrlStorage(
    ICacheStorage cacheStorage, 
    IGuidFactory guidFactory, 
    IMomentProvider momentProvider) : ICreateUrlStorage
{
    public async Task<Domain.Url> CreateUrl(UrlDto urlDto, CancellationToken ct = default)
    {
        var url = new Url
        {
            Id = guidFactory.Create(),
            Original = urlDto.OriginalUrl,
            Short = urlDto.UrlCode,
            CreatedAt = momentProvider.Current
        };
        var cachedUrl = await cacheStorage.Get<string>(url.Short, ct);
        if (cachedUrl != url.Original)
        {
            await cacheStorage.Set(url.Short, url.Original, TimeSpan.FromDays(7), ct);
        }

        return url.ToDomainModel();
    }
}