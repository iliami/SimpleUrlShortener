using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.BackgroundServices;

internal static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddBackgroundServices(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.AddHostedService<ExpiredUrlMappingProcessor>();
        return builder;
    }
}