using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;
using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Domain.Application;

public static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddApplication(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.Configure<UrlShortenerSettings>(
            builder.Configuration.GetSection(nameof(UrlShortenerSettings)));
        builder.Services.AddSingleton<UrlShortenerSettings>(sp =>
            sp.GetRequiredService<IOptions<UrlShortenerSettings>>().Value);

        builder.Services
            .AddMediator(options =>
            {
                options.Assemblies = [typeof(CreateShortUrlUseCase)];
                options.ServiceLifetime = ServiceLifetime.Scoped;
            });

        return builder;
    }
}