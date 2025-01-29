namespace SimpleUrlShortener.Domain.CreateUrlUseCase;

public record CreateUrlResponse(string OriginalUrl, string Code);