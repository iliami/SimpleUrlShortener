using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Storages;

public class CreateOrUpdateUrlMappingStorage(AppDbContext dbContext) : ICreateOrUpdateUrlMappingStorage
{
    public Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct)
        => dbContext.UrlMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code.Value, ct)
            .Map();

    public async Task<bool> Save(UrlMapping um, CancellationToken ct)
    {
        var entity = um.Map();
        // PostgreSQL upsert
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "analytics-collector"."UrlMapping" ("Code", "Original", "CreatedAt") 
            VALUES ({0}, {1}, {2})
            ON CONFLICT ("Code") 
            DO UPDATE SET
                "Original" = EXCLUDED."Original", 
                "CreatedAt" = EXCLUDED."CreatedAt";
            """,
            entity.Code,
            entity.Original,
            entity.CreatedAt);
        return true;
    }
}