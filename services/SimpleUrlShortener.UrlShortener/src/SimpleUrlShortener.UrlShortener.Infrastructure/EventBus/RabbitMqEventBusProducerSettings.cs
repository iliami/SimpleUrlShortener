namespace SimpleUrlShortener.UrlShortener.Infrastructure.EventBus;

public class RabbitMqEventBusProducerSettings
{
    public int PublishRetryCount { get => Math.Clamp(field, 1, 100); set; } = 5;
    public int PublishRetryIntervalMs { get => Math.Clamp(field, 0, 600000); set; } = 300;
    public int PublishConfirmTimeoutMs { get => Math.Clamp(field, 0, 600000); set; } = 5000;
    public bool UsePublisherConfirms { get; set; } = true;
    public bool UsePersistentMessages { get; set; } = true;
}