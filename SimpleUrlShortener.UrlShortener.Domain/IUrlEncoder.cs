namespace SimpleUrlShortener.UrlShortener.Domain;

public interface IUrlEncoder
{
    Task<string> Encode(string originalUrl, CancellationToken ct = default);
}