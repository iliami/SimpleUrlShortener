using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.EventBus.Shared.Events;

public record UrlCreatedIntegrationEvent : IntegrationEvent
{
    public UrlCreatedIntegrationEvent(UrlCreatedMessage message)
        : base(
            new UrlCreatedIntegrationEventMessage(message),
            "urls",
            RabbitMQ.Client.ExchangeType.Fanout,
            "url.created")
    {
    }

    private class UrlCreatedIntegrationEventMessage(UrlCreatedMessage message)
        : IntegrationEventMessage(message);
}