using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleUrlShortener.AnalyticsCollector.API.Endpoints;

namespace SimpleUrlShortener.AnalyticsCollector.API;

public static class EndpointExtensions
{
    public static (IServiceCollection Services, IConfiguration Configuration) AddEndpoints(
        this (IServiceCollection Services, IConfiguration Configuration) builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();

        var endpointServiceDescriptors = typeof(IEndpoint).Assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        builder.Services.TryAddEnumerable(endpointServiceDescriptors);

        return builder;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        var builder = routeGroupBuilder ?? (IEndpointRouteBuilder)app;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}

public static class EndpointRegistry
{
    private static readonly Dictionary<Assembly, List<Type>> EndpointTypes = new();

    public static void RegisterEndpoints(Assembly moduleAssembly, IEnumerable<Type> endpointTypes)
    {
        EndpointTypes[moduleAssembly] = endpointTypes.ToList();
    }

    public static IEnumerable<Type> GetEndpointTypes(Assembly moduleAssembly)
    {
        return EndpointTypes.TryGetValue(moduleAssembly, out var types)
            ? types
            : Enumerable.Empty<Type>();
    }
}