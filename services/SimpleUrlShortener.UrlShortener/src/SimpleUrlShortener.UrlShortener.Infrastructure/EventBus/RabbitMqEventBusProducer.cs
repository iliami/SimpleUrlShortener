using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Domain.Shared;
using SimpleUrlShortener.UrlShortener.Infrastructure.EventBus.Shared;
using SimpleUrlShortener.UrlShortener.Infrastructure.EventBus.Shared.Events;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.EventBus;

public class RabbitMqEventBusProducer(
    RabbitMqConnection connection,
    RabbitMqEventBusProducerSettings eventBusProducerSettings,
    ILogger<RabbitMqEventBusProducer> logger
) : IEventBus
{
    private static readonly Lazy<JsonSerializerOptions> JsonSerializerOptions = new(
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new EventBusMessagePolymorphicTypeResolver()
        });

    public async Task Publish(EventBusMessage eventBusMessage, CancellationToken ct = default)
    {
        var integrationEvent = eventBusMessage.ToIntegrationEvent();
        try
        {
            await using var channel = await CreateChannelAsync();

            await SetupExchange(channel, integrationEvent);

            var properties = CreateBasicProperties(integrationEvent.Message);

            logger.LogInformation(
                "Publishing event: {EventId}. Exchange: {ExchangeName}. Exchange type: {ExchangeType}. Routing key: {RoutingKey}",
                integrationEvent.Message.MessageId, 
                integrationEvent.ExchangeName, 
                integrationEvent.ExchangeType,
                integrationEvent.RoutingKey);

            await PublishEvent(
                channel,
                integrationEvent,
                properties: properties);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish event: {EventId}", integrationEvent.Message.MessageId);
            throw;
        }
    }

    private async Task<IChannel> CreateChannelAsync(bool usePublisherConfirms = true)
    {
        var conn = await connection.GetConnectionAsync();

        var channelOptions = new CreateChannelOptions(
            usePublisherConfirms && eventBusProducerSettings.UsePublisherConfirms,
            usePublisherConfirms && eventBusProducerSettings.UsePublisherConfirms // tracking для mandatory
        );

        var channel = await conn.CreateChannelAsync(channelOptions);

        return channel;
    }

    private Task SetupExchange(IChannel channel, IntegrationEvent integrationEvent)
    {
        try
        {
            return channel.ExchangeDeclareAsync(
                exchange: integrationEvent.ExchangeName,
                type: integrationEvent.ExchangeType,
                durable: integrationEvent.Durable,
                autoDelete: integrationEvent.AutoDelete,
                arguments: null);
        }
        catch (OperationInterruptedException ex)
        {
            logger.LogError(ex, "Failed to declare exchange: {ExchangeName}", integrationEvent.ExchangeName);
            throw;
        }
    }

    private async Task PublishEvent(
        IChannel channel,
        IntegrationEvent integrationEvent,
        BasicProperties? properties = null)
    {
        byte[] body;
        try
        {
            body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent.Message, JsonSerializerOptions.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to serialize event: {EventId}", integrationEvent.Message.MessageId);
            throw;
        }

        var attempts = eventBusProducerSettings.PublishRetryCount;
        var interval = eventBusProducerSettings.PublishRetryIntervalMs;

        while (attempts > 0)
        {
            attempts--;
            try
            {
                if (eventBusProducerSettings.UsePublisherConfirms)
                {
                    var publishTask = properties is null
                        ? channel
                            .BasicPublishAsync(
                                exchange: integrationEvent.ExchangeName,
                                routingKey: integrationEvent.RoutingKey,
                                body: body)
                            .AsTask()
                        : channel
                            .BasicPublishAsync(
                                exchange: integrationEvent.ExchangeName,
                                routingKey: integrationEvent.RoutingKey,
                                body: body,
                                mandatory: false,
                                basicProperties: properties)
                            .AsTask();

                    var timeoutTask = Task.Delay(eventBusProducerSettings.PublishConfirmTimeoutMs);

                    var completedTask = await Task.WhenAny(publishTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        throw new TimeoutException("Publish confirmation timeout");
                    }

                    await publishTask;
                }
                else
                {
                    if (properties is null)
                    {
                        await channel.BasicPublishAsync(
                            exchange: integrationEvent.ExchangeName,
                            routingKey: integrationEvent.RoutingKey,
                            body: body);
                    }
                    else
                    {
                        await channel.BasicPublishAsync(
                            exchange: integrationEvent.ExchangeName,
                            routingKey: integrationEvent.RoutingKey,
                            body: body,
                            mandatory: false,
                            basicProperties: properties);
                    }
                }

                break;
            }
            catch (TimeoutException ex)
            {
                logger.LogError(ex,
                    "Failed to publish message to exchange: {ExchangeName}, routing key: {RoutingKey} due to timeout",
                    integrationEvent.ExchangeName, integrationEvent.RoutingKey);
                if (attempts > 0)
                {
                    await Task.Delay(interval);
                }
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                logger.LogError(ex,
                    "Failed to publish message to exchange: {ExchangeName}, routing key: {RoutingKey}",
                    integrationEvent.ExchangeName, integrationEvent.RoutingKey);
                if (attempts > 0)
                {
                    await Task.Delay(interval);
                }
            }
        }
    }

    private BasicProperties CreateBasicProperties(IntegrationEventMessage message, bool persistent = true)
    {
        var properties = new BasicProperties();

        if (persistent && eventBusProducerSettings.UsePersistentMessages)
        {
            properties.DeliveryMode = DeliveryModes.Persistent;
        }

        properties.ContentType = "application/json";
        properties.Timestamp = new AmqpTimestamp(message.OccurredOn.ToUnixTimeSeconds());
        properties.MessageId = message.MessageId.ToString();

        return properties;
    }

    private static bool IsTransient(Exception ex) => ex switch
    {
        OperationInterruptedException => true,
        BrokerUnreachableException => true,
        IOException => true,
        _ => false
    };
}