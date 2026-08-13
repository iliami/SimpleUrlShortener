using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence;

namespace SimpleUrlShortener.UrlLifetimeManager.API.Extensions;

public static class CoreExtensions
{
    public static WebApplicationBuilder AddUrlLifetimeManager(this WebApplicationBuilder builder)
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