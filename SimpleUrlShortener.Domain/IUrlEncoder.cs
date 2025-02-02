namespace SimpleUrlShortener.Domain;

public interface IUrlEncoder
{
    Task<string> Encode(string originalUrl, CancellationToken ct = default);
}