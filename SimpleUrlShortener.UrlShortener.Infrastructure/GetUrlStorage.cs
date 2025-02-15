using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.UrlShortener.Domain;
using SimpleUrlShortener.UrlShortener.Domain.GetUrlUseCase;

namespace SimpleUrlShortener.UrlShortener.Infrastructure;

public class GetUrlStorage(ICacheStorage storage, NoTrackingDbContext dbContext) : IGetUrlStorage
{
    public async Task<Url?> GetUrl(string urlCode, CancellationToken ct = default)
    {
        var cachedUrl = await storage.Get<Url>(urlCode, ct);
        if (cachedUrl is not null)
        {
            return cachedUrl;
        }

        var storedUrl = await dbContext.Urls.FirstOrDefaultAsync(u => u.Code == urlCode, ct);
        if (storedUrl is null)
        {
            return null;
        }

        await storage.Set(urlCode, storedUrl, TimeSpan.FromHours(12), ct);
        return Url.Create(storedUrl.Original, storedUrl.Code);
    }
}