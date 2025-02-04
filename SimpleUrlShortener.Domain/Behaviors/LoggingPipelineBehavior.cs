using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.Domain.Shared;

namespace SimpleUrlShortener.Domain.Behaviors;

public class LoggingPipelineBehavior<TRequest, TResponse>(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly Stopwatch _stopwatch = new();
    private readonly TimeSpan _warningDuration = TimeSpan.FromSeconds(1);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Handling request of type: {RequestType}", typeof(TRequest).Name);
        logger.LogTrace("Request parameters {@values}", request);

        _stopwatch.Start();
        var response = await next();
        _stopwatch.Stop();

        var timeSpan = _stopwatch.Elapsed;

        if (timeSpan > _warningDuration)
        {
            logger.LogWarning("Request of type {RequestType} has been handled too long. Duration: {Duration} ms.",
                typeof(TRequest).Name, (long)timeSpan.TotalMilliseconds);
        }

        if (response.Error is null)
        {
            logger.LogInformation("Handled request of type {RequestType} successfully. Duration: {Duration} ms.",
                typeof(TRequest).Name, (long)timeSpan.TotalMilliseconds);
        }
        else
        {
            logger.LogInformation("Handled request of type {RequestType} with error. Duration: {Duration} ms.",
                typeof(TRequest).Name, (long)timeSpan.TotalMilliseconds);
        }

        logger.LogTrace("Response payload {@values}", response);

        return response;
    }
}