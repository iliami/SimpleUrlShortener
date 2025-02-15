namespace SimpleUrlShortener.UrlShortener.Domain;

public interface IEventBus
{
    Task Publish<T>(T message, CancellationToken ct = default) where T : class;
}