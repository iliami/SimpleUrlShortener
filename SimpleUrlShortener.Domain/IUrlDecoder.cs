namespace SimpleUrlShortener.Domain;

public interface IUrlDecoder
{
    Task<string?> Decode(string encodedUrl);
}