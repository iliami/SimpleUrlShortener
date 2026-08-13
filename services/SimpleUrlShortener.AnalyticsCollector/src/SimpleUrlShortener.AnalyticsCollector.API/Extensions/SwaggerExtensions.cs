namespace SimpleUrlShortener.AnalyticsCollector.API.Extensions;

public static class SwaggerExtensions
{
    public static WebApplicationBuilder AddSwaggerService(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen();

        return builder;
    }

    public static IApplicationBuilder UseSwaggerService(this IApplicationBuilder app)
    {
        app
            .UseSwagger()
            .UseSwaggerUI();

        return app;
    }
}