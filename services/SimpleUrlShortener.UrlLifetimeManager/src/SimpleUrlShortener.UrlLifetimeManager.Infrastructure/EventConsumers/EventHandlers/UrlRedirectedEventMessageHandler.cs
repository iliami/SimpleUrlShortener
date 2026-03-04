using Mediator;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Shared;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.EventHandlers;

public class UrlRedirectedEventMessageHandler(
    ILogger<UrlRedirectedEventMessageHandler> logger,
    IMediator mediator)
    : IEventMessageHandler<UrlRedirectedMessage>
{
    public async Task HandleAsync(UrlRedirectedMessage eventBusMessage, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing UrlRedirected event: InstancePrefix={InstancePrefix} UrlCode={UrlCode} OriginalUrl={OriginalUrl}",
            eventBusMessage.InstancePrefix,
            eventBusMessage.UrlCode,
            eventBusMessage.OriginalUrl);

        try
        {
            var useCaseRequest = new AddUrlMappingRedirectionUseCaseRequest(
                new UrlCode(eventBusMessage.InstancePrefix, eventBusMessage.UrlCode),
                eventBusMessage.CreatedAt);

            try
            {
                var useCaseResponse = await mediator.Send<AddUrlMappingRedirectionUseCaseResponse>(
                    useCaseRequest, cancellationToken);
                if (!useCaseResponse.Success)
                {
                    logger.LogCritical(
                        "Could not add url mapping redirection: InstancePrefix={InstancePrefix} UrlCode={UrlCode} OriginalUrl={OriginalUrl}",
                        eventBusMessage.InstancePrefix,
                        eventBusMessage.UrlCode,
                        eventBusMessage.OriginalUrl);
                }
            }
            catch (NotFoundException ex) when (ex.EntityType == typeof(UrlMapping))
            {
                var createUseCaseRequest = new CreateOrUpdateUrlMappingUseCaseRequest(
                    new UrlCode(eventBusMessage.InstancePrefix, eventBusMessage.UrlCode),
                    new OriginalUrl(eventBusMessage.OriginalUrl),
                    eventBusMessage.CreatedAt);

                var createUseCaseResponse = await mediator.Send<CreateOrUpdateUrlMappingUseCaseResponse>(
                    createUseCaseRequest, cancellationToken);

                if (!createUseCaseResponse.Success)
                {
                    logger.LogCritical(
                        "Could not create url mapping: InstancePrefix={InstancePrefix} UrlCode={UrlCode} OriginalUrl={OriginalUrl}",
                        eventBusMessage.InstancePrefix,
                        eventBusMessage.UrlCode,
                        eventBusMessage.OriginalUrl);
                    return;
                }

                var useCaseResponse = await mediator.Send<AddUrlMappingRedirectionUseCaseResponse>(
                    useCaseRequest, cancellationToken);
                if (!useCaseResponse.Success)
                {
                    logger.LogCritical(
                        "Could not add url mapping redirection: InstancePrefix={InstancePrefix} UrlCode={UrlCode} OriginalUrl={OriginalUrl}",
                        eventBusMessage.InstancePrefix,
                        eventBusMessage.UrlCode,
                        eventBusMessage.OriginalUrl);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Error {Exception} occured while adding url mapping redirection: InstancePrefix={InstancePrefix} UrlCode={UrlCode} OriginalUrl={OriginalUrl}",
                ex,
                eventBusMessage.InstancePrefix,
                eventBusMessage.UrlCode,
                eventBusMessage.OriginalUrl);

            throw;
        }
    }

    public Task HandleAsync(EventBusMessage eventBusMessage, CancellationToken cancellationToken = default)
        => eventBusMessage is UrlRedirectedMessage message
            ? HandleAsync(message, cancellationToken)
            : throw new ArgumentException($"{nameof(eventBusMessage)} must be of type {nameof(UrlRedirectedMessage)}");
}