using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.BackgroundServices;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Clients;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddInfrastructure(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.AddScoped<ILifetimeCalculator, LifetimeCalculator>();
        return builder
            .AddEventConsumers()
            .AddPersistence()
            .AddClients()
            .AddBackgroundServices();
    }
}