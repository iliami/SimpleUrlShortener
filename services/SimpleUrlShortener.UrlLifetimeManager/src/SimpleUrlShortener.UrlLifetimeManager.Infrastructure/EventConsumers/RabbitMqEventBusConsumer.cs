using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.EventHandlers;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.Shared;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.Shared.Events;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers;

public class RabbitMqEventBusConsumer(
    RabbitMqConnection connection,
    RabbitMqEventBusConsumerSettings settings,
    IEventMessageHandlerProvider handlerProvider,
    ILogger<RabbitMqEventBusConsumer> logger) : BackgroundService
{
    private const string ExchangeName = "urls";
    private const string DlxName = "urls.url-lifetime-manager.dlx";
    private const string RetryQueueName = "urls.url-lifetime-manager.retry";

    private const string QueueUrlCreated = "urls.url-lifetime-manager.url.created";
    private const string QueueUrlRedirected = "urls.url-lifetime-manager.url.redirected";

    private const string DlqUrlCreated = "urls.url-lifetime-manager.url.created.dlq";
    private const string DlqUrlRedirected = "urls.url-lifetime-manager.url.redirected.dlq";

    private const string RoutingKeyCreated = "url.created";
    private const string RoutingKeyRedirected = "url.redirected";

    private const string RetryCountHeader = "x-retry-count";

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
        await _channel!.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        await _channel.ExchangeDeclareAsync(
            exchange: DlxName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        var retryArgs = new Dictionary<string, object?>
        {
            { "x-message-ttl", settings.RetryQueueTtlMs },
            { "x-dead-letter-exchange", ExchangeName }
        };

        await _channel.QueueDeclareAsync(
            queue: RetryQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: retryArgs);

        await DeclareMainQueueAsync(QueueUrlCreated, RoutingKeyCreated);
        await DeclareMainQueueAsync(QueueUrlRedirected, RoutingKeyRedirected);

        await _channel.QueueDeclareAsync(
            queue: DlqUrlCreated,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueDeclareAsync(
            queue: DlqUrlRedirected,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: DlqUrlCreated,
            exchange: DlxName,
            routingKey: RoutingKeyCreated);

        await _channel.QueueBindAsync(
            queue: DlqUrlRedirected,
            exchange: DlxName,
            routingKey: RoutingKeyRedirected);

        logger.LogInformation("Queues, DLX, retry queue, and DLQs declared and bound successfully");
    }

    private async Task DeclareMainQueueAsync(string queueName, string routingKey)
    {
        var queueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", DlxName },
            { "x-dead-letter-routing-key", routingKey }
        };

        await _channel!.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs);

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: ExchangeName,
            routingKey: routingKey);
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
                await PublishToDlqAsync(args, cancellationToken);
                await _channel!.BasicAckAsync(args.DeliveryTag, false, cancellationToken);
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
                var retryCount = GetRetryCount(args.BasicProperties);

                if (retryCount > 0)
                {
                    await PublishToRetryQueueAsync(args, retryCount - 1, cancellationToken);
                }
                else
                {
                    await PublishToDlqAsync(args, cancellationToken);
                }

                await _channel!.BasicAckAsync(args.DeliveryTag, false, cancellationToken);
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

    private int GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is not null &&
            properties.Headers.TryGetValue(RetryCountHeader, out var value))
        {
            return value switch
            {
                long longValue => (int)longValue,
                int intValue => intValue,
                _ => settings.MaxRetries
            };
        }

        return settings.MaxRetries;
    }

    private async Task PublishToRetryQueueAsync(BasicDeliverEventArgs args, int retryCount,
        CancellationToken cancellationToken)
    {
        var properties = new BasicProperties(args.BasicProperties);
        var headers = properties.Headers != null 
            ? new Dictionary<string, object?>(properties.Headers) 
            : new Dictionary<string, object?>();
    
        headers[RetryCountHeader] = retryCount;
        properties.Headers = headers;

        await _channel!.BasicPublishAsync(
            exchange: "",
            routingKey: RetryQueueName,
            mandatory: false,
            basicProperties: properties,
            body: args.Body,
            cancellationToken);

        logger.LogWarning(
            "Message NACKed and published to retry queue with {RetryCount} retries remaining: {MessageId}",
            retryCount,
            args.BasicProperties.MessageId);
    }

    private async Task PublishToDlqAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        var properties = new BasicProperties(args.BasicProperties);

        await _channel!.BasicPublishAsync(
            exchange: DlxName,
            routingKey: args.RoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: args.Body,
            cancellationToken);

        logger.LogWarning("Message sent to DLQ with routing key: {RoutingKey} (MessageId: {MessageId})",
            args.RoutingKey,
            args.BasicProperties.MessageId);
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