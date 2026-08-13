using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure;

public class UrlMappingRedirectionCoordinatesEnricher(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int maxBatchSize = 500;
        const int defaultBatchSize = 200;
        const int batchSizeIncreaseStep = 50;
        var tenMinutes = TimeSpan.FromMinutes(10);
        var thirtySeconds = TimeSpan.FromSeconds(30);

        var batchSize = defaultBatchSize;
        var interval = TimeSpan.FromMinutes(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var logger =
                scope.ServiceProvider.GetRequiredService<ILogger<UrlMappingRedirectionCoordinatesEnricher>>();

            try
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var request = new EnrichRedirectionsWithCoordinatesUseCaseRequest(batchSize);

                logger.LogInformation(
                    "Starting enrichment batch. BatchSize={BatchSize}, CurrentInterval={Interval} minutes",
                    batchSize, interval.TotalMinutes);

                var response = await mediator.Send(request, stoppingToken);

                if (response.TotalProcessed != 0)
                {
                    var successRate = response.TotalProcessed > 0
                        ? (double)response.SuccessfullyEnriched / response.TotalProcessed * 100
                        : 0;

                    logger.LogInformation(
                        "Batch processing completed. Processed={TotalProcessed}, SuccessfullyEnriched={SuccessfullyEnriched}, Failed={FailedCount}, SuccessRate={SuccessRate:F2}%",
                        response.TotalProcessed,
                        response.SuccessfullyEnriched,
                        response.TotalProcessed - response.SuccessfullyEnriched,
                        successRate);

                    if (response.TotalProcessed != response.SuccessfullyEnriched)
                    {
                        logger.LogWarning("Some redirections failed to enrich with coordinates.");
                    }
                }

                if (response.TotalProcessed == 0)
                {
                    if (interval < tenMinutes)
                    {
                        interval += thirtySeconds;
                    }

                    batchSize = defaultBatchSize;

                    logger.LogInformation(
                        "No redirections found for coordinate enrichment. Increasing retry interval to {NewInterval} minutes. Next attempt will use batch size of {BatchSize}.",
                        interval.TotalMinutes, defaultBatchSize);
                }
                else if (response.TotalProcessed == batchSize)
                {
                    interval = TimeSpan.FromMinutes(1);
                    if (batchSize < maxBatchSize)
                    {
                        var newBatchSize = Math.Min(batchSize + batchSizeIncreaseStep, maxBatchSize);
                        logger.LogInformation(
                            "Full batch processed. Increasing batch size from {OldBatchSize} to {NewBatchSize}.",
                            batchSize, newBatchSize);
                        batchSize = newBatchSize;
                    }
                    else
                    {
                        logger.LogDebug("Full batch processed. Batch size already at maximum ({MaxBatchSize}).",
                            maxBatchSize);
                    }
                }
                else
                {
                    interval = TimeSpan.FromMinutes(1);
                    if (batchSize != defaultBatchSize)
                    {
                        logger.LogInformation(
                            "Partial batch processed ({ProcessedCount}/{BatchSize}). Resetting batch size to default ({DefaultBatchSize}).",
                            response.TotalProcessed, batchSize, defaultBatchSize);
                    }

                    batchSize = defaultBatchSize;
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("IMediator",
                                                           StringComparison.OrdinalIgnoreCase))
            {
                logger.LogCritical(ex,
                    "Dependency injection error: IMediator service could not be resolved.");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation(
                    "Background service is shutting down gracefully. Current state: BatchSize={BatchSize}, Interval={Interval} minutes.",
                    batchSize, interval.TotalMinutes);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Unexpected error during coordinate enrichment. Will retry after {RetryInterval} minutes with batch size of {BatchSize}.",
                    interval.TotalMinutes,
                    batchSize);
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                logger.LogDebug(
                    "Waiting {Interval} minutes before next enrichment cycle.",
                    interval.TotalMinutes);

                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}