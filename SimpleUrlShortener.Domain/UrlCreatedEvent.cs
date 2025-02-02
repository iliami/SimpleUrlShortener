namespace SimpleUrlShortener.Domain;

public class UrlCreatedEvent
{
    public string Original { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public static class UrlCreatedEventExtensions
{
    public static UrlCreatedEvent ToEventModel(this Url url) => new()
    {
        Original = url.Original,
        Code = url.Code,
        CreatedAt = DateTimeOffset.UtcNow,
    };
}