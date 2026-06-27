using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.EventBus.Shared.Events;

public static class IntegrationEventFactory
{
    public static IntegrationEvent ToIntegrationEvent(this EventBusMessage eventBusMessage) 
        => eventBusMessage switch
        {
            UrlCreatedMessage message => From(message),
            UrlRedirectedMessage message => From(message),
            UrlDeletedMessage message => From(message),
            _ => throw new NotImplementedException()
        };

    private static IntegrationEvent From(UrlCreatedMessage message)
    {
        return new IntegrationEvent(
            new IntegrationEventMessage(message),
            "urls",
            RabbitMQ.Client.ExchangeType.Direct,
            "url.created");
    }
    
    private static IntegrationEvent From(UrlRedirectedMessage message)
    {
        return new IntegrationEvent(
            new IntegrationEventMessage(message),
            "urls",
            RabbitMQ.Client.ExchangeType.Direct,
            "url.redirected");
    }

    private static IntegrationEvent From(UrlDeletedMessage message)
    {
        return new IntegrationEvent(
            new IntegrationEventMessage(message),
            "urls",
            RabbitMQ.Client.ExchangeType.Direct,
            "url.deleted");
    }
}