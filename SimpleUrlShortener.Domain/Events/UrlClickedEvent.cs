namespace SimpleUrlShortener.Domain.Events;

public record UrlClickedEvent(string Original, string Code, DateTimeOffset ClickedAt);