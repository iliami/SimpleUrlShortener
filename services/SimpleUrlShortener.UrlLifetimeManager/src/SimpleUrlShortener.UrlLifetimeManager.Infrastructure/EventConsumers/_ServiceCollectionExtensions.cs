using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.EventHandlers;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.Shared;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers;

internal static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddEventConsumers(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.Configure<RabbitMqConnectionSettings>(
            builder.Configuration.GetSection(nameof(RabbitMqConnectionSettings)));
        builder.Services.AddSingleton<RabbitMqConnectionSettings>(sp =>
            sp.GetRequiredService<IOptions<RabbitMqConnectionSettings>>().Value);
        builder.Services.AddSingleton<RabbitMqConnection>();

        builder.Services.Configure<RabbitMqEventBusConsumerSettings>(
            builder.Configuration.GetSection(nameof(RabbitMqEventBusConsumerSettings)));
        builder.Services.AddSingleton<RabbitMqEventBusConsumerSettings>(sp =>
            sp.GetRequiredService<IOptions<RabbitMqEventBusConsumerSettings>>().Value);
        builder.Services.AddHostedService<RabbitMqEventBusConsumer>();


        var handlerType = typeof(IEventMessageHandler<>);
        var handlerTypeAssemblyClasses = handlerType.Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false });

        foreach (var type in handlerTypeAssemblyClasses)
        {
            var interfaces = type.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                if (!@interface.IsGenericType ||
                    @interface.GetGenericTypeDefinition() != handlerType)
                {
                    continue;
                }

                builder.Services.TryAddTransient(@interface, type);
            }
        }

        builder.Services.AddSingleton<IEventMessageHandlerProvider, EventMessageHandlerProvider>();

        return builder;
    }
}