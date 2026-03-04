using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Clients;

internal static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddClients(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        var urlShortenerAddressString = builder.Configuration["Clients:UrlShortener:Url"]
                                        ?? throw new Exception("UrlShortenerAddress is missing");
        urlShortenerAddressString = urlShortenerAddressString.Trim().TrimEnd("/").ToString() + "/";

        var urlShortenerApiKey = builder.Configuration["Clients:UrlShortener:ApiKey"];

        builder.Services
            .AddHttpClient<IUrlShortenerClient, UrlShortenerHttpClient>(
                "url-shortener",
                client =>
                {
                    client.BaseAddress = new Uri(urlShortenerAddressString);
                    if (urlShortenerApiKey is not null)
                    {
                        client.DefaultRequestHeaders.Add("X-API-KEY", urlShortenerApiKey);
                    }
                })
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