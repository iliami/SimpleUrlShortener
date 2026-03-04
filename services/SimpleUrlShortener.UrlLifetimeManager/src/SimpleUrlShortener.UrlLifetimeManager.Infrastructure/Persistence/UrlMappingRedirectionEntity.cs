using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence;

public class UrlMappingRedirectionEntity
{
    public Guid Id { get; init; }
    public DateTimeOffset OccuredOn { get; init; }
}

public static class UrlMappingRedirectionsEntityMapExtensions
{
    public static UrlMappingRedirectionEntity Map(this UrlMappingRedirection umr)
        => new()
        {
            OccuredOn = umr.OccuredOn.ToUniversalTime()
        };

    public static UrlMappingRedirection Map(this UrlMappingRedirectionEntity entity)
        => new(entity.OccuredOn.ToUniversalTime());

    public static async Task<UrlMappingRedirection?> Map(this Task<UrlMappingRedirectionEntity?> task)
    {
        var entity = await task;
        return entity?.Map();
    }
}