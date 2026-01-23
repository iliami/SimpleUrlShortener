using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Infrastructure.EventBus;
using SimpleUrlShortener.UrlShortener.Infrastructure.Persistence;

namespace SimpleUrlShortener.UrlShortener.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddInfrastructure(this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.AddPersistence();
        builder.AddEventBus();

        builder.Services.AddSingleton<IUrlEncoder, UrlEncoder>();

        return builder;
    }
}