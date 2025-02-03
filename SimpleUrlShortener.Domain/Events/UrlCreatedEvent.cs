namespace SimpleUrlShortener.Domain.Events;

public record UrlCreatedEvent(string Original, string Code, DateTimeOffset CreatedAt);

public static class UrlCreatedEventExtensions
{
    public static UrlCreatedEvent ToEventModel(this Url url)
        => new(
            url.Original,
            url.Code,
            DateTimeOffset.UtcNow);
}