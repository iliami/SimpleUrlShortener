using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.BackgroundServices;

public class ExpiredUrlMappingProcessor(
    ILogger<ExpiredUrlMappingProcessor> logger, 
    IServiceProvider serviceProvider)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await DoWork(mediator, logger, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogCritical("Exception occured: {Exception}", ex);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private static async Task DoWork(
        IMediator mediator,
        ILogger<ExpiredUrlMappingProcessor> logger,
        CancellationToken cancellationToken)
    {
        var getExpiredUrlMappingsListUseCaseRequest = new GetExpiredUrlMappingsListUseCaseRequest(100);
        var getExpiredUrlMappingsListUseCaseResponse = await mediator.Send(getExpiredUrlMappingsListUseCaseRequest, cancellationToken);
        foreach (var code in getExpiredUrlMappingsListUseCaseResponse.UrlCodes)
        {
            var updateUrlMappingLifetimeUseCaseRequest = new UpdateUrlMappingLifetimeUseCaseRequest(code);
            try
            {
                await mediator.Send(updateUrlMappingLifetimeUseCaseRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError("Exception occured while updating lifetime of the url mapping {UrlCode}: {Exception}", 
                    code, ex);
            }
        }
    }
}