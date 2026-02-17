using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Storages;

public class GetPublicInfoStorage(AppDbContext dbContext) : IGetPublicInfoStorage
{
    public async Task<(long TotalRedirectionCount, DateTimeOffset LastRedirectionDate)> GetInfo(
        UrlCode urlCode,
        CancellationToken cancellationToken)
    {
        var result = await dbContext.UrlMappings
            .AsNoTracking()
            .Where(um => um.Code == urlCode.Value)
            .Select(um => new
            {
                TotalRedirectionCount = um.UrlMappingRedirections.Count,
                LastRedirectionDate = um.UrlMappingRedirections.Max(umr => umr.OccuredOn)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result is null
            ? (0, DateTimeOffset.MinValue)
            : (result.TotalRedirectionCount, result.LastRedirectionDate);
    }
}