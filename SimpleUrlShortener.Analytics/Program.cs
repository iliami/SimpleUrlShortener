using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Serilog;
using SimpleUrlShortener.Analytics.Domain.Behaviors;
using SimpleUrlShortener.Analytics.Domain.UseCases.GetAll;
using SimpleUrlShortener.Analytics.Infrastructure;
using SimpleUrlShortener.Analytics.Infrastructure.Storages;
using SimpleUrlShortener.Analytics.Presentation;
using SimpleUrlShortener.Analytics.Presentation.Features;
using SimpleUrlShortener.Analytics.Presentation.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

builder.Services
    .AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.Duration | HttpLoggingFields.RequestPath |
                                HttpLoggingFields.RequestBody | HttpLoggingFields.RequestHeaders |
                                HttpLoggingFields.ResponseBody | HttpLoggingFields.ResponseHeaders;
    })

    .AddValidatorsFromAssemblyContaining<SimpleUrlShortener.Analytics.Domain.Url>()
    .AddMediatR(configurator =>
    {
        configurator.RegisterServicesFromAssemblyContaining<SimpleUrlShortener.Analytics.Domain.Url>();
        configurator.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
        configurator.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
    })

    .AddDbContext<NoTrackingDbContext>()
    .AddMemoryCache()
    .AddScoped<IGetAllStorage, GetAllStorage>()
    .AddKeyedScoped<SimpleUrlShortener.Analytics.Domain.IConverter<IEnumerable<SimpleUrlShortener.Analytics.Domain.Url>>, SimpleUrlShortener.Analytics.Infrastructure.CsvConverter>("CsvConverter")
    
    .AddEndpoints()
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1",
            new OpenApiInfo { Title = "SimpleUrlShortener.Analytics", Version = "v1" });

        options.AddSecurityDefinition(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "API Key Authorization header using the special scheme.",
                Name = ApiKeyAuthenticationDefaults.ApiKeyHeaderName,
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = ApiKeyAuthenticationDefaults.ApiKeyHeaderName,
                Scheme = ApiKeyAuthenticationDefaults.AuthenticationScheme
            });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = ApiKeyAuthenticationDefaults.AuthenticationScheme
                    }
                },
                []
            }
            
        });
    })
    .AddScoped<IApiKeyValidator, ApiKeyValidator>()
    .Configure<ApiSettings>(builder.Configuration.GetSection(nameof(ApiSettings)))
    .AddAuthorization()
    .AddAuthentication(options => { options.DefaultScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme; })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationDefaults.AuthenticationScheme,
        _ => { });


var app = builder.Build();

var apiPrefix = app.MapGroup("api/");

app.MapEndpoints(apiPrefix);


app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();

        var endpointServiceDescriptors = typeof(IEndpoint).Assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(endpointServiceDescriptors);

        return services;
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