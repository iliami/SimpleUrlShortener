using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Infrastructure;

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

app.UseExceptionHandler("/error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();