using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using SimpleUrlShortener.UrlShortener.API;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Infrastructure;
using SimpleUrlShortener.UrlShortener.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, loggerConfiguration) => { loggerConfiguration.ReadFrom.Configuration(context.Configuration); },
    false,
    true);

builder.Host.UseDefaultServiceProvider((_, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var serviceName = builder.Configuration["ThisService:Name"] ?? "UrlShortener";
var serviceVersion = builder.Configuration["ThisService:Version"] ?? "unknown";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName
        }))
    .UseOtlpExporter()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddRabbitMQInstrumentation()
        .AddSource($"SimpleUrlShortener.{serviceName}.*")
    )
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
    )
    .WithLogging();

builder.Services
    .AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.Duration | HttpLoggingFields.RequestPath |
                                HttpLoggingFields.RequestBody | HttpLoggingFields.RequestHeaders |
                                HttpLoggingFields.ResponseBody | HttpLoggingFields.ResponseHeaders;
    })
    .AddSwaggerGen();

var di = (builder.Services, builder.Configuration);

di.AddApplication().AddInfrastructure().AddEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

var apiPrefix = app.MapGroup("api/");

app.MapEndpoints(apiPrefix);

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();