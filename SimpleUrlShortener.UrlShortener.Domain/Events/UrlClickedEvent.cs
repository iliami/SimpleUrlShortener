namespace SimpleUrlShortener.UrlShortener.Domain.Events;

public record UrlClickedEvent(string Original, string Code)
{
    public DateTimeOffset ClickedAt { get; } = DateTimeOffset.UtcNow;
}

public static class UrlClickedEventExtensions
{
    public static UrlClickedEvent ToEventModel(this Url url)
        => new UrlClickedEvent(url.Original, url.Code);
}