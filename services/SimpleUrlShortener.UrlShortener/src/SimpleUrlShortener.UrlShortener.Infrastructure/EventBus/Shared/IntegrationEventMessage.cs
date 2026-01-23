using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.EventBus.Shared;

/// <summary>
/// The wrapper for the domain message
/// <br />
/// An instance of this class will be published on the event bus
/// </summary>
/// <param name="message">The domain message</param>
public abstract class IntegrationEventMessage(EventBusMessage message)
{
    public virtual Guid MessageId { get; init; } = Guid.NewGuid();
    public string MessageType { get; init; } = message.GetType().Name;
    public EventBusMessage Message { get; init; } = message;

    public virtual DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    public virtual int Version { get; init; } = 1;
    public virtual Dictionary<string, object>? Metadata { get; init; }
}