using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application;
using SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers;
using SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddInfrastructure(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.AddScoped<IGeoIpService, SimpleGeoIpService>();
        return builder.AddEventConsumers().AddPersistence();
    }
}