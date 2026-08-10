using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace SimpleUrlShortener.UrlShortener.API.Extensions;

public static class MonitoringExtensions
{
    public static WebApplicationBuilder AddMonitoring(this WebApplicationBuilder builder)
        => builder.WithSerilog().WithOpenTelemetry();

    private static WebApplicationBuilder WithSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog(
            (context, loggerConfiguration) => { loggerConfiguration.ReadFrom.Configuration(context.Configuration); },
            false,
            true);

        builder.Services
            .AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.Duration | HttpLoggingFields.RequestPath |
                                        HttpLoggingFields.RequestBody | HttpLoggingFields.RequestHeaders |
                                        HttpLoggingFields.ResponseBody | HttpLoggingFields.ResponseHeaders;
            });

        return builder;
    }

    private static WebApplicationBuilder WithOpenTelemetry(this WebApplicationBuilder builder)
    {
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
                .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options => options.EnrichWithIDbCommand = (activity, command) =>
                {
                    activity.AddTag("db.query", command.CommandText);
                    activity.AddTag("db.provider", "Npgsql");
                })
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

        return builder;
    }
}