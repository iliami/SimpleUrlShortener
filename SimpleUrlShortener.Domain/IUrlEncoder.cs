namespace SimpleUrlShortener.Domain;

public interface IUrlEncoder
{
    Task<string> Encode(string url);
}