using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Application;

public interface IGeoIpService
{
    Task<Coordinates> GetCoordinates(Ip ip, CancellationToken cancellationToken);
}