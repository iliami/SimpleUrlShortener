using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence;

public class UrlMappingRedirectionEntity
{
    public Guid Id { get; init; }
    public DateTimeOffset OccuredOn { get; init; }
    public string Ip { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

public static class UrlMappingRedirectionsEntityMapExtensions
{
    public static UrlMappingRedirectionEntity Map(this UrlMappingRedirection umr)
        => new()
        {
            OccuredOn = umr.OccuredOn.ToUniversalTime(),
            Ip = umr.Ip.Value,
            Latitude = umr.Coordinates.Latitude,
            Longitude = umr.Coordinates.Longitude
        };

    public static UrlMappingRedirection Map(this UrlMappingRedirectionEntity entity)
        => new(entity.OccuredOn.ToUniversalTime(), new Ip(entity.Ip), new Coordinates(entity.Latitude, entity.Longitude));

    public static async Task<UrlMappingRedirection?> Map(this Task<UrlMappingRedirectionEntity?> task)
    {
        var entity = await task;
        return entity?.Map();
    }
}