using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Infrastructure;
using SimpleUrlShortener.UrlShortener.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

builder.Services
    .AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.Duration | HttpLoggingFields.RequestPath |
                                HttpLoggingFields.RequestBody | HttpLoggingFields.RequestHeaders |
                                HttpLoggingFields.ResponseBody | HttpLoggingFields.ResponseHeaders;
    })
    .AddControllersWithViews();

var di = (builder.Services, builder.Configuration);

di.AddApplication().AddInfrastructure();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler("/error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();