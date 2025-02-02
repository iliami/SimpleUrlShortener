using SimpleUrlShortener.Domain;
using SimpleUrlShortener.Domain.CreateUrlUseCase;

namespace SimpleUrlShortener.Infrastructure;

public class CreateUrlStorage(
    IStringCacheStorage stringCacheStorage, 
    IGuidFactory guidFactory, 
    IMomentProvider momentProvider) : ICreateUrlStorage
{
    public async Task<Domain.Url> CreateUrl(UrlDto urlDto, CancellationToken ct = default)
    {
        var url = new Url
        {
            Id = guidFactory.Create(),
            Original = urlDto.OriginalUrl,
            Code = urlDto.UrlCode,
            CreatedAt = momentProvider.Current
        };
        await stringCacheStorage.Set(url.Code, url.Original, TimeSpan.FromHours(12), ct);

        return url.ToDomainModel();
    }
}