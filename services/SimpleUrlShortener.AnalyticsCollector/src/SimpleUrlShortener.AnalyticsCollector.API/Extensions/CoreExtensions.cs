using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application;
using SimpleUrlShortener.AnalyticsCollector.Infrastructure;
using SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence;

namespace SimpleUrlShortener.AnalyticsCollector.API.Extensions;

public static class CoreExtensions
{
    public static WebApplicationBuilder AddAnalyticsCollector(this WebApplicationBuilder builder)
    {
        builder.Host.UseDefaultServiceProvider((_, options) =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        var di = (builder.Services, builder.Configuration);

        di.AddApplication().AddInfrastructure().AddEndpoints();

        return builder;
    }

    public static WebApplication MigrateIfDevelopment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        return app;
    }
}