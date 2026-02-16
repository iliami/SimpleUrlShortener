using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Application;

public static class ServiceCollectionExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddApplication(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services
            .AddMediator(options =>
            {
                options.Assemblies = [typeof(CreateOrUpdateUrlMappingUseCase)];
                options.ServiceLifetime = ServiceLifetime.Scoped;
            });

        return builder;
    }
}