using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Infrastructure.EventBus.Shared;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.EventBus;

internal static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddEventBus(this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.Configure<RabbitMqConnectionSettings>(
            builder.Configuration.GetSection(nameof(RabbitMqConnectionSettings)));
        builder.Services.AddSingleton<RabbitMqConnectionSettings>(sp =>
            sp.GetRequiredService<IOptions<RabbitMqConnectionSettings>>().Value);
        builder.Services.AddSingleton<RabbitMqConnection>();

        builder.Services.Configure<RabbitMqEventBusProducerSettings>(
            builder.Configuration.GetSection(nameof(RabbitMqEventBusProducerSettings)));
        builder.Services.AddSingleton<RabbitMqEventBusProducerSettings>(sp =>
            sp.GetRequiredService<IOptions<RabbitMqEventBusProducerSettings>>().Value);
        builder.Services.AddSingleton<IEventBus, RabbitMqEventBusProducer>();

        return builder;
    }
}