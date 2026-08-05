using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Core;

public record UrlMappingRedirection(Guid Id, DateTimeOffset OccuredOn, Ip Ip);

public record UrlMappingRedirectionWithCoordinates(Guid Id, DateTimeOffset OccuredOn, Ip Ip, Coordinates Coordinates)
    : UrlMappingRedirection(Id, OccuredOn, Ip);

public record struct Coordinates(
    [property: Range(-90, 90)] double Latitude,
    [property: Range(-180, 180)] double Longitude);

public record struct Ip
{
    public string Value { get; }
    public IpKind Kind { get; }

    public Ip(string input)
    {
        var ip = IPAddress.Parse(input);
        var kind = IpKind.DetermineKind(ip);

        Value = ip.ToString();
        Kind = kind;
    }

    public Ip(string input, string kind)
    {
        var ip = IPAddress.Parse(input);
        Value = ip.ToString();
        Kind = IpKind.Parse(kind);
    }
}

public abstract record IpKind(string Value) : IEquatable<string>
{
    private record IpKindIpv4Public() : IpKind("Ipv4Public");

    private record IpKindIpv4Private() : IpKind("Ipv4Private");

    private record IpKindIpv6Public() : IpKind("Ipv6Public");

    private record IpKindIpv6Private() : IpKind("Ipv6Private");

    public static IpKind Ipv4Public { get; } = new IpKindIpv4Public();
    public static IpKind Ipv4Private { get; } = new IpKindIpv4Private();
    public static IpKind Ipv6Public { get; } = new IpKindIpv6Public();
    public static IpKind Ipv6Private { get; } = new IpKindIpv6Private();

    public static IpKind Parse(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        if (input == Ipv4Public) return Ipv4Public;
        if (input == Ipv4Private) return Ipv4Private;
        if (input == Ipv6Public) return Ipv6Public;
        if (input == Ipv6Private) return Ipv6Private;

        throw new ArgumentException($"Unknown IpKind: {input}");
    }

    public static IpKind DetermineKind(IPAddress ip)
    {
        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Адреса, для которых определение геопозиции и других внешних характеристик невозможно
            if (IPAddress.IsLoopback(ip) || // ::1/128
                ip.IsIPv6LinkLocal || // fe80::/10
                ip.IsIPv6UniqueLocal || // fc00::/7
                ip.IsIPv6Multicast || // ff00::/8
                ip.Equals(IPAddress.IPv6Any)) // ::/128 (Unspecified)
            {
                return Ipv6Private;
            }

            // Если это IPv4-адрес, отображённый в формате IPv6 (::ffff:a.b.c.d), 
            // делегируем проверку логике для IPv4
            if (ip.IsIPv4MappedToIPv6)
            {
                return DetermineKind(ip.MapToIPv4());
            }

            return Ipv6Public;
        }

        if (ip.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new ArgumentException($"Unsupported AddressFamily: {ip.AddressFamily}");
        }

        if (IPAddress.IsLoopback(ip))
        {
            return Ipv4Private;
        }

        var b = ip.GetAddressBytes();
        // Приватные (или зарезервированные) диапазоны IPv4
        if (b[0] == 0 || // 0.0.0.0/8
            b[0] == 10 || // 10.0.0.0/8
            (b[0] == 172 && b[1] >= 16 && b[1] <= 31) || // 172.16.0.0/12
            (b[0] == 192 && b[1] == 168) || // 192.168.0.0/16
            (b[0] == 169 && b[1] == 254) || // 169.254.0.0/16 (Link-local)
            (b[0] >= 224 && b[0] <= 239) || // Multicast 224.0.0.0/4
            b[0] >= 240) // Reserved 240.0.0.0/4
        {
            return Ipv4Private;
        }

        return Ipv4Public;
    }

    public virtual bool Equals(string? other) => Value.Equals(other, StringComparison.OrdinalIgnoreCase);

    public static bool operator ==(IpKind kind, string str) => kind.Equals(str);
    public static bool operator !=(IpKind kind, string str) => !kind.Equals(str);
    public static bool operator ==(string str, IpKind kind) => kind.Equals(str);
    public static bool operator !=(string str, IpKind kind) => !kind.Equals(str);
}