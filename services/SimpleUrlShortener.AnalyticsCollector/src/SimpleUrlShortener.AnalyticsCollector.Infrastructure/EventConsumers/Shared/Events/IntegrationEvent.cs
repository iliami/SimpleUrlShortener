namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers.Shared.Events;

/// <summary>
/// The wrapper for the integration message
/// <br />
/// This class is needed for get metadata about the integration event
/// </summary>
public record IntegrationEvent(
    IntegrationEventMessage Message,
    string ExchangeName,
    string ExchangeType,
    string RoutingKey,
    bool Durable = true,
    bool AutoDelete = false);