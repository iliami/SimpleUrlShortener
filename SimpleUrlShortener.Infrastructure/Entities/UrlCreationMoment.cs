namespace SimpleUrlShortener.Infrastructure.Entities;

public class UrlCreationMoment
{
    public Guid Id { get; init; } = Guid.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public required Url Url { get; init; }
}