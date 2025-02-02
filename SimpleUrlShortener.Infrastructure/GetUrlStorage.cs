using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.Domain.GetUrlUseCase;

namespace SimpleUrlShortener.Infrastructure;

public class GetUrlStorage(IStringCacheStorage storage, NoTrackingDbContext dbContext) : IGetUrlStorage
{
    public async Task<string?> GetUrl(string urlCode, CancellationToken ct = default)
    {
        var cachedUrl = await storage.Get(urlCode, ct);
        if (cachedUrl is not null)
        {
            return cachedUrl;
        }

        var storedUrl = await dbContext.Urls.FirstOrDefaultAsync(u => u.Code == urlCode, ct);
        if (storedUrl is null)
        {
            return null;
        }

        await storage.Set(urlCode, storedUrl.Original, TimeSpan.FromHours(12), ct);
        return storedUrl.Original;
    }
}