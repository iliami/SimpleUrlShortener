namespace SimpleUrlShortener.UrlShortener.Domain.GetUrlUseCase;

public record GetUrlResponse(string OriginalUrl, string Code);