using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Storages;

public class DeleteUrlMappingStorage(
    AppDbContext dbContext) : IDeleteUrlMappingStorage
{
    public Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct)
        => dbContext.UrlMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code.Value, ct)
            .Map();

    public async Task<bool> Save(UrlMapping urlMapping, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = urlMapping.Map();
            dbContext.UrlMappings.Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.Entry(entity).State = EntityState.Detached;

            return true;
        }
        catch
        {
            return false;
        }
    }
}