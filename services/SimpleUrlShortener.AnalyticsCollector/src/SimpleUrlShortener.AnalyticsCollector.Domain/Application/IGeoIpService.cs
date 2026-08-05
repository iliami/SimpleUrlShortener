using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Application;

public interface IGeoIpService
{
    Task<Dictionary<Ip, Coordinates>> GetCoordinates(IEnumerable<Ip> ips, CancellationToken cancellationToken);
}