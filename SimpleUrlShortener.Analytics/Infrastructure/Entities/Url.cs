namespace SimpleUrlShortener.Analytics.Infrastructure.Entities;

public class Url
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Original { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public ICollection<UrlCreationMoment> CreationMoments { get; init; } = [];
    public ICollection<UrlClick> ClickMoments { get; init; } = [];
}