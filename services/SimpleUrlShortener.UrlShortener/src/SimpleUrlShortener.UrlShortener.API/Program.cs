using SimpleUrlShortener.UrlShortener.API.Extensions;

var app = WebApplication.CreateBuilder(args)
    .AddUrlShortener()
    .AddMonitoring()
    .AddSwaggerService()
    .Build();

app
    .MigrateIfDevelopment()
    .MapEndpoints("api/")
    .UseForwardedHeaders()
    .UseSwaggerService();

app.Run();