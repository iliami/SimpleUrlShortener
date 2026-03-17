using Mediator;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;
using SimpleUrlShortener.AnalyticsCollector.Domain.Shared;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers.EventHandlers;

public class UrlDeletedEventMessageHandler(
    ILogger<UrlDeletedEventMessageHandler> logger,
    IMediator mediator)
    : IEventMessageHandler<UrlDeletedMessage>
{
    public async Task HandleAsync(UrlDeletedMessage eventBusMessage, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing UrlDeleted event: InstancePrefix={InstancePrefix} UrlCode={UrlCode}}",
            eventBusMessage.InstancePrefix,
            eventBusMessage.UrlCode);

        try
        {
            var useCaseRequest = new DeleteUrlMappingRequest(
                new UrlCode(eventBusMessage.InstancePrefix, eventBusMessage.UrlCode));

            try
            {
                await mediator.Send(
                    useCaseRequest, cancellationToken);
            }
            catch (NotFoundException ex) when (ex.EntityType == typeof(UrlMapping))
            {
                logger.LogCritical(
                    "Could not find url mapping: InstancePrefix={InstancePrefix} UrlCode={UrlCode}",
                    eventBusMessage.InstancePrefix,
                    eventBusMessage.UrlCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Error {Exception} occured while adding url mapping redirection: InstancePrefix={InstancePrefix} UrlCode={UrlCode} OriginalUrl={OriginalUrl}",
                ex,
                eventBusMessage.InstancePrefix,
                eventBusMessage.UrlCode);

            throw;
        }
    }

    public Task HandleAsync(EventBusMessage eventBusMessage, CancellationToken cancellationToken = default)
        => eventBusMessage is UrlDeletedMessage message
            ? HandleAsync(message, cancellationToken)
            : throw new ArgumentException($"{nameof(eventBusMessage)} must be of type {nameof(UrlDeletedMessage)}");
}