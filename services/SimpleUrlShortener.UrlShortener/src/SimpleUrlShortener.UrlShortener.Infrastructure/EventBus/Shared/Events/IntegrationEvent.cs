using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.EventBus.Shared.Events;

/// <summary>
/// The wrapper for the integration message
/// <br />
/// This class is needed for get metadata about the integration event
/// </summary>
public abstract record IntegrationEvent(
    IntegrationEventMessage Message,
    string ExchangeName,
    string ExchangeType,
    string RoutingKey,
    bool Durable = true,
    bool AutoDelete = false)
{
    public static IntegrationEvent From(EventBusMessage message) => message switch
    {
        UrlCreatedMessage urlCreatedMessage => new UrlCreatedIntegrationEvent(urlCreatedMessage),
        UrlRedirectedMessage urlRedirectedMessage => new UrlRedirectedIntegrationEvent(urlRedirectedMessage),
        _ => throw new NotImplementedException()
    };
}