namespace SimpleUrlShortener.UrlShortener.Infrastructure.Entities;

public class UrlClick
{
    public Guid Id { get; set; }
    public DateTimeOffset ClickedAt { get; set; }
    public required Url Url { get; set; }
}