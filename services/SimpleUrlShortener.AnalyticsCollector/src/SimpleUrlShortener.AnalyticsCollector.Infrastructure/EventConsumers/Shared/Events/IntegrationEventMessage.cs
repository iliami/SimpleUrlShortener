using SimpleUrlShortener.AnalyticsCollector.Domain.Shared;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers.Shared.Events;

/// <summary>
/// The wrapper for the domain message
/// <br />
/// An instance of this class will be published on the event bus
/// </summary>
/// <param name="message">The domain message</param>
public sealed class IntegrationEventMessage(EventBusMessage message)
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public string MessageType { get; init; } = message.GetType().Name;
    public EventBusMessage Message { get; init; } = message;

    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    public int Version { get; init; } = 1;
    public Dictionary<string, object>? Metadata { get; init; }
}