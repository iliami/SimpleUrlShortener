using System.Net;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Clients;

internal static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddClients(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        return builder.AddIpApiComClient();
    }

    private static (IServiceCollection Services, IConfiguration Configuration) AddIpApiComClient(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.AddTransient<IpApiRateLimitGuardHandler>();

        var httpClientBuilder = builder.Services
            .AddHttpClient<IGeoIpService, IpApiComService>(
                "IpApiCom",
                (_, client) =>
                {
                    client.BaseAddress = new Uri("http://ip-api.com/");
                    // Большой таймаут, так как retry и ожидание в rate limiter могут занять время
                    client.Timeout = TimeSpan.FromMinutes(5);
                });

        httpClientBuilder.AddResilienceHandler(
            "IpApiCom",
            (pipelineBuilder, context) =>
            {
                var logger = context.ServiceProvider.GetService<ILogger<IpApiComService>>();

                pipelineBuilder
                    .AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        Delay = TimeSpan.FromSeconds(2),

                        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                            .Handle<HttpRequestException>()
                            .Handle<TimeoutRejectedException>()
                            .HandleResult(response =>
                                response.StatusCode == HttpStatusCode.TooManyRequests ||
                                response.StatusCode == HttpStatusCode.RequestTimeout ||
                                (int)response.StatusCode >= 500),

                        // Специальная задержка для 429 - ждём X-Ttl секунд (или 60 при отсутствии заголовка)
                        DelayGenerator = args =>
                        {
                            if (args.Outcome.Result is not { StatusCode: HttpStatusCode.TooManyRequests } response)
                            {
                                return ValueTask.FromResult<TimeSpan?>(null);
                            }

                            var delay = GetThrottleDelay(response) ?? TimeSpan.FromSeconds(60);
                            return ValueTask.FromResult<TimeSpan?>(delay);
                        },

                        OnRetry = args =>
                        {
                            logger.LogWarning("Retry attempt {Attempt} after {Delay}ms for status {Status}",
                                args.AttemptNumber + 1, args.RetryDelay.TotalMilliseconds,
                                args.Outcome.Result?.StatusCode);
                            return default;
                        }
                    })
                    .AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 15,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 1000
                    }))
                    .AddTimeout(TimeSpan.FromSeconds(30));
            });

        httpClientBuilder.AddHttpMessageHandler<IpApiRateLimitGuardHandler>();

        return builder;
    }

    private static TimeSpan? GetThrottleDelay(HttpResponseMessage response)
    {
        // Проверка стандартого Retry-After
        if (response.Headers.RetryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
        {
            return delta;
        }

        if (response.Headers.RetryAfter?.Date is { } retryDate)
        {
            var delay = retryDate - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                return delay;
            }
        }

        // Проверка специфичного для ip-api.com заголовка X-Ttl
        if (response.Headers.TryGetValues("X-Ttl", out var ttlValues) &&
            int.TryParse(ttlValues.FirstOrDefault(), out var ttlSeconds) &&
            ttlSeconds > 0)
        {
            return TimeSpan.FromSeconds(ttlSeconds);
        }

        return null;
    }
}