using Mediator;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Shared;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.EventHandlers;

public class UrlCreatedEventMessageHandler(
    ILogger<UrlCreatedEventMessageHandler> logger,
    IMediator mediator) : IEventMessageHandler<UrlCreatedMessage>
{
    public async Task HandleAsync(UrlCreatedMessage eventBusMessage, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing UrlCreated event: InstancePrefix={InstancePrefix} UrlCode={UrlCode} OriginalUrl={OriginalUrl}",
            eventBusMessage.InstancePrefix,
            eventBusMessage.UrlCode,
            eventBusMessage.OriginalUrl);

        try
        {
            var useCaseRequest = new CreateOrUpdateUrlMappingUseCaseRequest(
                new UrlCode(eventBusMessage.InstancePrefix, eventBusMessage.UrlCode),
                new OriginalUrl(eventBusMessage.OriginalUrl),
                eventBusMessage.CreatedAt);

            var useCaseResponse = await mediator.Send(useCaseRequest, cancellationToken);

            if (!useCaseResponse.Success)
            {
                logger.LogCritical(
                    "Could not create url mapping: InstancePrefix={InstancePrefix} UrlCode={UrlCode} OriginalUrl={OriginalUrl}",
                    eventBusMessage.InstancePrefix,
                    eventBusMessage.UrlCode,
                    eventBusMessage.OriginalUrl);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Error {Exception} occured while creating url mapping: InstancePrefix={InstancePrefix} UrlCode={UrlCode} OriginalUrl={OriginalUrl}",
                ex,
                eventBusMessage.InstancePrefix,
                eventBusMessage.UrlCode,
                eventBusMessage.OriginalUrl);

            throw;
        }
    }

    public Task HandleAsync(EventBusMessage eventBusMessage, CancellationToken cancellationToken = default)
        => eventBusMessage is UrlCreatedMessage message
            ? HandleAsync(message, cancellationToken)
            : throw new ArgumentException($"{nameof(eventBusMessage)} must be of type {nameof(UrlCreatedMessage)}");
}