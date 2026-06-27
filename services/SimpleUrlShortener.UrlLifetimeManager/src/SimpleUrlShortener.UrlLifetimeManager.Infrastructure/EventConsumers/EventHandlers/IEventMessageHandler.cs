using SimpleUrlShortener.UrlLifetimeManager.Domain.Shared;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.EventHandlers;

public interface IEventMessageHandler
{
    Task HandleAsync(EventBusMessage eventBusMessage, CancellationToken cancellationToken = default);
}

public interface IEventMessageHandler<in TEventMessage> : IEventMessageHandler
    where TEventMessage : EventBusMessage
{
    Task HandleAsync(TEventMessage eventBusMessage, CancellationToken cancellationToken = default);
}