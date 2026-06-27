using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence.Storages;

public class GetExpiredUrlMappingsListStorage(
    AppDbContext dbContext)
    : IGetExpiredUrlMappingsListStorage
{
    public Task<UrlCode[]> GetExpiredUrlMappingsList(DateTimeOffset now, int limit, CancellationToken cancellationToken)
        => dbContext.UrlMappings
            .Where(x => x.ExpiresAt < now)
            .Take(limit)
            .Select(x => new UrlCode(x.Code))
            .ToArrayAsync(cancellationToken);
}