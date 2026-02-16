using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Storages;

public class AddUrlMappingRedirectionStorage(AppDbContext dbContext)
    : IAddUrlMappingRedirectionStorage
{
    public Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct)
        => dbContext.UrlMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code.Value, ct)
            .Map();

    public async Task<bool> Save(UrlMapping um, CancellationToken ct)
    {
        var entity = um.Map();
        dbContext.UrlMappings.Update(entity);
        await dbContext.SaveChangesAsync(ct);
        dbContext.UrlMappings.Entry(entity).State = EntityState.Detached;
        return true;
    }
}