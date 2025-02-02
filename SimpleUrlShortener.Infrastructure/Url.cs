namespace SimpleUrlShortener.Infrastructure;

public class Url
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Original { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public static class UrlMappers
{
    public static Domain.Url ToDomainModel(this Url url)
        => Domain.Url.Create(url.Id, url.Original, url.Code, url.CreatedAt);
}