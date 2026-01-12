using MassTransit;
using SimpleUrlShortener.UrlShortener.Domain;

namespace SimpleUrlShortener.UrlShortener.Infrastructure;

public class EventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public Task Publish<T>(T message, CancellationToken ct = default) where T : class
        => publishEndpoint.Publish(message, ct);
}