using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application;
using SimpleUrlShortener.AnalyticsCollector.Infrastructure.EventConsumers;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence;

internal static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddPersistence(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.AddDbContext<AppDbContext>();

        builder.AddStorages();

        return builder;
    }

    private static (IServiceCollection Services, IConfiguration Configuration) AddStorages(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        var baseStorageType = typeof(IStorage);

        var storageInterfaces = baseStorageType.Assembly.GetTypes()
            .Where(x => x is { IsInterface: true, IsPublic: true })
            .Where(x => x != baseStorageType && x.IsAssignableTo(baseStorageType));

        var storageImplementations = typeof(AppDbContext).Assembly.GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false, IsPublic: true })
            .Where(x => x.IsAssignableTo(baseStorageType))
            .ToArray();

        foreach (var @interface in storageInterfaces)
        {
            var implementation = storageImplementations
                .SingleOrDefault(i => i.GetInterfaces().Contains(@interface));
            if (implementation is null)
            {
                continue;
            }

            builder.Services.TryAddScoped(@interface, implementation);
        }

        return builder;
    }
}