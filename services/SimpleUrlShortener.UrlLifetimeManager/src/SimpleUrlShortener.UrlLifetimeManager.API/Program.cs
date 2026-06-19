using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SimpleUrlShortener.UrlLifetimeManager.API;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = true;

    options.ValidateOnBuild = true;
});

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