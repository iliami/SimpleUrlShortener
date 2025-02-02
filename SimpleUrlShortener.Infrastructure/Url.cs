namespace SimpleUrlShortener.Infrastructure;

public class Url
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Original { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public ICollection<UrlCreationMoment> CreationMoments { get; init; } = [];
}

public class UrlCreationMoment
{
    public Guid Id { get; init; } = Guid.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public required Url Url { get; init; }
}