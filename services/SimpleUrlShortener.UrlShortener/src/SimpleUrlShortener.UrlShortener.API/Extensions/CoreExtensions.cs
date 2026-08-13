using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Infrastructure;
using SimpleUrlShortener.UrlShortener.Infrastructure.Persistence;

namespace SimpleUrlShortener.UrlShortener.API.Extensions;

public static class CoreExtensions
{
    public static WebApplicationBuilder AddUrlShortener(this WebApplicationBuilder builder)
    {
        builder.Host.UseDefaultServiceProvider((_, options) =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        var di = (builder.Services, builder.Configuration);

        di.AddApplication().AddInfrastructure().AddEndpoints();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto |
                                       ForwardedHeaders.XForwardedHost;

            /*
             TODO [SECURITY] [CRITICAL]:
               Очистка KnownIPNetworks и KnownProxies создает уязвимость к подделке заголовков
               X-Forwarded-* (IP/Host Spoofing), так как приложение начинает доверять им от любых источников.
               После утверждения инфраструктуры необходимо заменить этот код на явное указание
               доверенных IP-адресов или сетей прокси.
            */
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

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