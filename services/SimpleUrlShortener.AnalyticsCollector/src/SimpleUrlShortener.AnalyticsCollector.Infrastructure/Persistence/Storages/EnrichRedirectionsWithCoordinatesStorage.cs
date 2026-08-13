using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Storages;

public class EnrichRedirectionsWithCoordinatesStorage(
    AppDbContext dbContext) : IEnrichRedirectionsWithCoordinatesStorage
{
    public Task<UrlMappingRedirection[]> GetPublicIpRedirectionsWithoutCoordinates(int batchSize, CancellationToken ct)
        => dbContext.UrlMappingRedirections
            .Where(x =>
                x.IpKind == IpKind.Ipv4Public.Value ||
                x.IpKind == IpKind.Ipv6Public.Value)
            .Where(x =>
                x.Latitude == null &&
                x.Longitude == null)
            .OrderBy(x => x.Id)
            .Take(batchSize)
            .Select(x => x.Map())
            .ToArrayAsync(ct);

    public Task<int> Save(UrlMappingRedirectionWithCoordinates[] redirections, CancellationToken ct)
    {
        var entities = redirections.Select(x => x.Map());
        dbContext.UrlMappingRedirections.AttachRange(entities);
        return dbContext.SaveChangesAsync(ct);
    }
}