using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence;

public class UrlMappingRedirectionEntity
{
    public Guid Id { get; init; }
    public DateTimeOffset OccuredOn { get; init; }
    public string Ip { get; init; } = string.Empty;
    public string IpKind { get; init; } = string.Empty;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    
    public string? UrlMappingId { get; init; }
    public Guid? UrlMappingDeletionId { get; init; }
}

public static class UrlMappingRedirectionsEntityMapExtensions
{
    public static UrlMappingRedirectionEntity Map(this UrlMappingRedirection umr) => umr switch
    {
        UrlMappingRedirectionWithCoordinates x => x.Map(),
        _ => new UrlMappingRedirectionEntity
        {
            OccuredOn = umr.OccuredOn.ToUniversalTime(),
            Ip = umr.Ip.Value,
            IpKind = umr.Ip.Kind.Value,
        }
    };

    public static UrlMappingRedirectionEntity Map(this UrlMappingRedirectionWithCoordinates umr)
        => new()
        {
            Id = umr.Id,
            OccuredOn = umr.OccuredOn.ToUniversalTime(),
            Ip = umr.Ip.Value,
            IpKind = umr.Ip.Kind.Value,
            Latitude = umr.Coordinates.Latitude,
            Longitude = umr.Coordinates.Longitude
        };

    public static UrlMappingRedirection Map(this UrlMappingRedirectionEntity entity)
        => entity.Latitude is not null && entity.Longitude is not null
            ? new UrlMappingRedirectionWithCoordinates(entity.Id, entity.OccuredOn.ToUniversalTime(),
                new Ip(entity.Ip, entity.IpKind),
                new Coordinates(entity.Latitude.Value, entity.Longitude.Value))
            : new UrlMappingRedirection(entity.Id, entity.OccuredOn.ToUniversalTime(),
                new Ip(entity.Ip, entity.IpKind));

    public static async Task<UrlMappingRedirection?> Map(this Task<UrlMappingRedirectionEntity?> task)
    {
        var entity = await task;
        return entity?.Map();
    }
}