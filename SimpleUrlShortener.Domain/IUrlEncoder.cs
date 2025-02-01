namespace SimpleUrlShortener.Domain;

public interface IUrlEncoder
{
    Task<string> Encode(string normalizedUrl, CancellationToken ct = default);
}