namespace SimpleUrlShortener.Domain;

public class UrlCreatedEvent
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Original { get; init; } = string.Empty;
    public string Normalized { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public static class UrlCreatedEventExtensions
{
    public static UrlCreatedEvent ToEventModel(this Url url) => new()
    {
        Id = url.Id,
        Original = url.Original,
        Normalized = url.Normalized,
        Code = url.Code,
        CreatedAt = url.CreatedAt,
    };
}