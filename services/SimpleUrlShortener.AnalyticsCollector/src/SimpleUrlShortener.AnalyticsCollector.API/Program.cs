using SimpleUrlShortener.AnalyticsCollector.API.Extensions;

var app = WebApplication.CreateBuilder(args)
    .AddAnalyticsCollector()
    .AddMonitoring()
    .AddSwaggerService()
    .Build();

app
    .MigrateIfDevelopment()
    .MapEndpoints("api/")
    .UseSwaggerService();

app.Run();