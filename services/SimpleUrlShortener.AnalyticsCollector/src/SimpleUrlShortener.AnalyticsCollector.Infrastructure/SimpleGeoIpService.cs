using SimpleUrlShortener.AnalyticsCollector.Domain.Application;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure;

public class SimpleGeoIpService : IGeoIpService
{
    public Task<Coordinates> GetCoordinates(Ip ip, CancellationToken cancellationToken)
    {
        var (latitude, longitude) = ip.Kind switch
        {
            IpKind.Ipv4Public => (55.751244, 37.618423), // Moscow
            IpKind.Ipv4Private => (40.711967, -74.006076), // New York
            IpKind.Ipv6 => (39.916668, 116.383331), // Beijing
            _ => throw new ArgumentOutOfRangeException(nameof(ip), ip, "Invalid ip kind")
        };

        return Task.FromResult(new Coordinates(latitude, longitude));
    }
}