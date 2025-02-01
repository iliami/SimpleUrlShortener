namespace SimpleUrlShortener.Domain;

public record UrlDto(string OriginalUrl, string NormalizedUrl, string UrlCode);