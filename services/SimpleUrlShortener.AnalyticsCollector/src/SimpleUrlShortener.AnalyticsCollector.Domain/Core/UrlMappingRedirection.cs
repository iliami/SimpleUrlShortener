using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Core;

public record UrlMappingRedirection(DateTimeOffset OccuredOn, Ip Ip, Coordinates Coordinates);

public record struct Coordinates(
    [property: Range(-90, 90)] double Latitude,
    [property: Range(-180, 180)] double Longitude);

public readonly struct Ip
{
    public string Value { get; }
    public IpKind Kind { get; }

    public Ip(string input)
    {
        var ip = IPAddress.Parse(input);
        var kind = DetermineKind(ip);

        Value = ip.ToString();
        Kind = kind;
    }

    private static IpKind DetermineKind(IPAddress ip)
    {
        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return IpKind.Ipv6;
        }

        if (IPAddress.IsLoopback(ip))
        {
            return IpKind.Ipv4Private;
        }

        var b = ip.GetAddressBytes();
        // Приватные (или зарезервированные) диапазоны
        if (b[0] == 0 || // 0.0.0.0/8
            b[0] == 10 || // 10.0.0.0/8
            (b[0] == 172 && b[1] >= 16 && b[1] <= 31) || // 172.16.0.0/12
            (b[0] == 192 && b[1] == 168) || // 192.168.0.0/16
            (b[0] == 169 && b[1] == 254) || // 169.254.0.0/16
            (b[0] >= 224 && b[0] <= 239) || // Multicast 224.0.0.0/4
            b[0] >= 240) // Reserved 240.0.0.0/4
        {
            return IpKind.Ipv4Private;
        }

        return IpKind.Ipv4Public;
    }
}

public enum IpKind
{
    Ipv4Public,
    Ipv4Private,
    Ipv6
}