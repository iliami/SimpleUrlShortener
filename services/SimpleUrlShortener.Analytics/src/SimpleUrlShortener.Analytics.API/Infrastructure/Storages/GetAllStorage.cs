using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SimpleUrlShortener.Analytics.Domain.UseCases.GetAll;

namespace SimpleUrlShortener.Analytics.Infrastructure.Storages;

public class GetAllStorage(NoTrackingDbContext dbContext, IMemoryCache memoryCache) : IGetAllStorage
{
    public async Task<IEnumerable<Domain.Url>> GetAll(string search = "", CancellationToken ct = default)
        => (await memoryCache.GetOrCreateAsync(
            nameof(GetAllStorage.GetAll) + search,
            async factory =>
            {
                var data = await dbContext.Urls
                    .Where(u => search == "" || EF.Functions.ILike(u.Original, $"%{search}%"))
                    .Select(u => Domain.Url.Create(
                        u.Original, u.Code,
                        u.CreationMoments
                            .Select(cm => cm.CreatedAt),
                        u.ClickMoments
                            .Select(cm => cm.ClickedAt)))
                    .ToArrayAsync(ct);

                factory.SetValue(data);
                factory.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                return data;
            }))!;
}