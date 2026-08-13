using SimpleUrlShortener.UrlLifetimeManager.API.Extensions;

var app = WebApplication.CreateBuilder(args)
    .AddUrlLifetimeManager()
    .AddMonitoring()
    .AddSwaggerService()
    .Build();

app
    .MigrateIfDevelopment()
    .MapEndpoints("api/")
    .UseSwaggerService();

app.Run();