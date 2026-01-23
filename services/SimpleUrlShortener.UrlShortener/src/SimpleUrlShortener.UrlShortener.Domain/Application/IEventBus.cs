using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Domain.Application;

public interface IEventBus
{
    Task Publish(EventBusMessage eventBusMessage, CancellationToken ct = default);
}