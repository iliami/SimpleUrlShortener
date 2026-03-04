using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence.Storages;

public class UpdateUrlMappingLifetimeStorage(
    AppDbContext dbContext)
    : IUpdateUrlMappingLifetimeStorage
{
    public Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct)
        => dbContext.UrlMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code.Value, ct)
            .Map();

    public async Task<bool> Delete(UrlCode code, CancellationToken ct)
    {
        var count = await dbContext.UrlMappings
            .Where(x => x.Code == code.Value)
            .ExecuteDeleteAsync(ct);
        return count == 1;
    }

    public async Task<bool> Save(UrlMapping um, CancellationToken ct)
    {
        var entity = um.Map();
        dbContext.UrlMappings.Update(entity);
        await dbContext.SaveChangesAsync(ct);
        dbContext.UrlMappings.Entry(entity).State = EntityState.Detached;
        return true;
    }
}