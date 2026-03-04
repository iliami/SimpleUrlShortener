using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence.Storages;

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
            INSERT INTO "url-lifetime-manager"."UrlMapping" ("Code", "Original", "CreatedAt", "ExpiresAt") 
            VALUES ({0}, {1}, {2}, {3})
            ON CONFLICT ("Code") 
            DO UPDATE SET
                "Original" = EXCLUDED."Original", 
                "CreatedAt" = EXCLUDED."CreatedAt",
                "ExpiresAt" = EXCLUDED."ExpiresAt";
            """,
            entity.Code,
            entity.Original,
            entity.CreatedAt,
            entity.ExpiresAt);
        return true;
    }
}