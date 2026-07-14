namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers;

public class RabbitMqEventBusConsumerSettings
{
    public int RequeueDelayMs { get; set; } = 1000;
    public bool AutoAck { get; set; } = false;
    public ushort PrefetchCount { get; set; } = 10;
    public int RetryQueueTtlMs { get; set; } = 5000;
    public int MaxRetries { get; set; } = 5;
}