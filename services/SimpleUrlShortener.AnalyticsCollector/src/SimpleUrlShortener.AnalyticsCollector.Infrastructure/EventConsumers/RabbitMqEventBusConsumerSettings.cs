namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers;

public class RabbitMqEventBusConsumerSettings
{
    public int RequeueDelayMs { get; set; } = 1000;
    public bool AutoAck { get; set; } = false;
    public ushort PrefetchCount { get; set; } = 10;
}