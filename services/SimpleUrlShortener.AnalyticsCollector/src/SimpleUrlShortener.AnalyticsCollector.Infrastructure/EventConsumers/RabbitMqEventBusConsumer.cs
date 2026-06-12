using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers.EventHandlers;
using SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers.Shared;
using SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers.Shared.Events;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers;

public class RabbitMqEventBusConsumer(
    RabbitMqConnection connection,
    RabbitMqEventBusConsumerSettings settings,
    IEventMessageHandlerProvider handlerProvider,
    ILogger<RabbitMqEventBusConsumer> logger) : BackgroundService
{
    private const string QueueUrlCreated = "analytics-collector.url.created";
    private const string QueueUrlRedirected = "analytics-collector.url.redirected";
    private const string QueueUrlDeleted = "analytics-collector.url.deleted";

    private static readonly Lazy<JsonSerializerOptions> JsonSerializerOptions = new(
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new EventBusMessagePolymorphicTypeResolver()
        });

    private IConnection? _consumerConnection;
    private IChannel? _channel;
    private readonly CancellationTokenSource _shutdownToken = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeAsync(stoppingToken);

        await using var registration = stoppingToken.Register(() => _shutdownToken.Cancel());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("Consumer is stopping...");
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _consumerConnection = await connection.GetConnectionAsync();

        _channel = await _consumerConnection.CreateChannelAsync(
            new CreateChannelOptions(false, false),
            cancellationToken);

        await _channel.BasicQosAsync(0, settings.PrefetchCount, false, cancellationToken);

        await DeclareAndBindQueuesAsync();
        await StartConsumingAsync(cancellationToken);
    }

    private async Task DeclareAndBindQueuesAsync()
    {
        const string ExchangeName = "urls";
        await _channel!.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        await _channel.QueueDeclareAsync(
            queue: QueueUrlCreated,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: QueueUrlCreated,
            exchange: ExchangeName,
            routingKey: "url.created");

        await _channel.QueueDeclareAsync(
            queue: QueueUrlRedirected,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: QueueUrlRedirected,
            exchange: ExchangeName,
            routingKey: "url.redirected");

        await _channel.QueueDeclareAsync(
            queue: QueueUrlDeleted,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: QueueUrlDeleted,
            exchange: ExchangeName,
            routingKey: "url.deleted");

        logger.LogInformation("Queues declared and bound successfully");
    }

    private async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ShutdownAsync += (_, args) =>
        {
            logger.LogWarning("Cannel shutdown: {Exception}", args.Exception);
            return Task.CompletedTask;
        };

        consumer.RegisteredAsync += (_, args) =>
        {
            logger.LogInformation("Consumer registered: {ConsumerTags}", args.ConsumerTags);
            return Task.CompletedTask;
        };

        consumer.UnregisteredAsync += (_, args) =>
        {
            logger.LogWarning("Consumer unregistered: {ConsumerTags}", args.ConsumerTags);
            return Task.CompletedTask;
        };

        consumer.ReceivedAsync += async (_, args) => { await HandleDeliveryAsync(args, cancellationToken); };

        await _channel!.BasicConsumeAsync(
            queue: QueueUrlCreated,
            autoAck: settings.AutoAck,
            consumer: consumer,
            cancellationToken: cancellationToken);

        await _channel!.BasicConsumeAsync(
            queue: QueueUrlRedirected,
            autoAck: settings.AutoAck,
            consumer: consumer,
            cancellationToken: cancellationToken);

        await _channel!.BasicConsumeAsync(
            queue: QueueUrlDeleted,
            autoAck: settings.AutoAck,
            consumer: consumer,
            cancellationToken: cancellationToken);

        logger.LogInformation("Consumer started successfully");
    }

    private async Task HandleDeliveryAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        var messageId = args.BasicProperties.MessageId;
        var routingKey = args.RoutingKey;

        try
        {
            var body = args.Body.Span;

            var integrationEventMessage = JsonSerializer.Deserialize<IntegrationEventMessage>(
                body,
                JsonSerializerOptions.Value);

            if (integrationEventMessage is null)
            {
                logger.LogWarning("Failed to deserialize message: {MessageId}", messageId);
                await NackAndRequeueAsync(args.DeliveryTag);
                return;
            }

            logger.LogInformation(
                "Received event: {EventType} (MessageId: {MessageId}, RoutingKey: {RoutingKey})",
                integrationEventMessage.Message,
                messageId,
                routingKey);

            await DispatchEventAsync(integrationEventMessage, cancellationToken);

            if (!settings.AutoAck)
            {
                await _channel!.BasicAckAsync(args.DeliveryTag, false, cancellationToken);
                logger.LogDebug("Message acknowledged: {MessageId}", messageId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message: {MessageId}", messageId);

            if (!settings.AutoAck)
            {
                await NackAndRequeueAsync(args.DeliveryTag);
            }
        }
    }

    private async Task DispatchEventAsync(IntegrationEventMessage integrationEventMessage,
        CancellationToken cancellationToken)
    {
        var eventType = integrationEventMessage.Message.GetType();
        var handler = handlerProvider.GetHandler(integrationEventMessage.Message);
        if (handler is not null)
        {
            await handler.HandleAsync(integrationEventMessage.Message, cancellationToken);
        }
        else
        {
            logger.LogWarning("No handler found for event type: {EventType}", eventType.Name);
        }
    }

    private async Task NackAndRequeueAsync(ulong deliveryTag)
    {
        try
        {
            await _channel!.BasicNackAsync(deliveryTag, false, true);

            await Task.Delay(settings.RequeueDelayMs, _shutdownToken.Token);

            logger.LogDebug("Message NACKed and requeued: {DeliveryTag}", deliveryTag);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to NACK message: {DeliveryTag}", deliveryTag);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping RabbitMQ consumer...");

        await _shutdownToken.CancelAsync();

        try
        {
            if (_channel is { IsOpen: true })
            {
                await _channel.CloseAsync(cancellationToken);
                _channel.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error closing consumer channel");
        }

        await base.StopAsync(cancellationToken);
    }
}