namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Clients.UrlShortener;

public class UrlShortenerOptions
{
    public string Url { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
}