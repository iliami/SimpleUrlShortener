using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleUrlShortener.UrlShortener.Domain.Application;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.Persistence;

internal static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddPersistence(this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.AddDbContext<AppDbContext>();
        builder.Services.AddScoped<IShardProvider, SimpleShardProvider>();
        builder.Services.AddScoped<IUrlWriteRepository, SimpleUrlRepository>();
        builder.Services.AddScoped<IUrlReadRepository, SimpleUrlRepository>();

        return builder;
    }
}