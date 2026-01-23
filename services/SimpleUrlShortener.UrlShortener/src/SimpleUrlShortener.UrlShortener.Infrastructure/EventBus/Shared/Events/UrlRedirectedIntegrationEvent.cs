using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.EventBus.Shared.Events;

public record UrlRedirectedIntegrationEvent : IntegrationEvent
{
    public UrlRedirectedIntegrationEvent(UrlRedirectedMessage message)
        : base(
            new UrlRedirectedIntegrationEventMessage(message),
            "urls",
            RabbitMQ.Client.ExchangeType.Fanout,
            "url.redirected")
    {
    }

    private class UrlRedirectedIntegrationEventMessage(UrlRedirectedMessage message)
        : IntegrationEventMessage(message);
}