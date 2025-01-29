using MassTransit;
using SimpleUrlShortener.Domain;

namespace SimpleUrlShortener.Infrastructure;

public class EventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public Task Publish<T>(T message, CancellationToken ct = default) where T : class
        => publishEndpoint.Publish(message, ct);
}