using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Clients.UrlShortener;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Clients;

internal static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddClients(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        return builder.AddUrlShortenerClient();
    }

    private static (IServiceCollection Services, IConfiguration Configuration) AddUrlShortenerClient(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.Configure<UrlShortenerOptions>(
            builder.Configuration.GetSection("Clients:UrlShortener"));

        builder.Services.AddTransient<UrlShortenerApiKeyAuthHandler>();

        builder.Services
            .AddHttpClient<IUrlShortenerClient, UrlShortenerHttpClient>(
                "url-shortener",
                (sp, client) =>
                {
                    var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<UrlShortenerOptions>>();
                    var options = optionsMonitor.CurrentValue;

                    if (string.IsNullOrWhiteSpace(options.Url))
                    {
                        throw new InvalidOperationException("UrlShortener Url is missing in configuration.");
                    }

                    var formattedUrl = options.Url.Trim().TrimEnd('/') + "/";
                    client.BaseAddress = new Uri(formattedUrl);
                })
            .AddHttpMessageHandler<UrlShortenerApiKeyAuthHandler>()
            .AddResilienceHandler(
                "url-shortener",
                pipelineBuilder =>
                {
                    pipelineBuilder
                        .AddTimeout(TimeSpan.FromSeconds(30))
                        .AddRetry(new HttpRetryStrategyOptions
                        {
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                .Handle<HttpRequestException>(),
                            BackoffType = DelayBackoffType.Exponential,
                            MaxRetryAttempts = 3,
                            Delay = TimeSpan.FromMilliseconds(100),
                            MaxDelay = TimeSpan.FromSeconds(10),
                        })
                        .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                        {
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                .Handle<HttpRequestException>(),
                            SamplingDuration = TimeSpan.FromSeconds(30),
                            FailureRatio = 0.5,
                            MinimumThroughput = 5,
                            BreakDuration = TimeSpan.FromSeconds(30),
                        });
                });

        return builder;
    }
}