using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using SimpleUrlShortener.UrlShortener.Domain;
using SimpleUrlShortener.UrlShortener.Domain.Behaviors;
using SimpleUrlShortener.UrlShortener.Domain.CreateUrlUseCase;
using SimpleUrlShortener.UrlShortener.Domain.GetUrlUseCase;
using SimpleUrlShortener.UrlShortener.Infrastructure;
using SimpleUrlShortener.UrlShortener.Infrastructure.Consumers;
using SimpleUrlShortener.UrlShortener.Presentation.Options;

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
    .AddSingleton<RabbitMqOptions>(sp => sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value)
    .Configure<RabbitMqOptions>(builder.Configuration.GetSection(nameof(RabbitMqOptions)))
    .AddValidatorsFromAssemblyContaining<SimpleUrlShortener.UrlShortener.Domain.Url>()
    .AddMediatR(configurator =>
    {
        configurator.RegisterServicesFromAssemblyContaining<SimpleUrlShortener.UrlShortener.Domain.Url>();
        configurator.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
        configurator.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
    })
    .AddMassTransit(busConfigurator =>
    {
        busConfigurator.SetSnakeCaseEndpointNameFormatter();

        busConfigurator.UsingRabbitMq((context, configurator) =>
        {
            var settings = context.GetRequiredService<RabbitMqOptions>();

            configurator.Host(settings.Host, settings.Port, settings.VirtualHost, "SimpleUrlShortener",
                hostConfigurator =>
                {
                    hostConfigurator.Username(settings.Username);
                    hostConfigurator.Password(settings.Password);
                });

            configurator.ConfigureEndpoints(context);
        });

        busConfigurator.AddConsumer<UrlClickedEventConsumer>();
    })
    .AddDbContext<NoTrackingDbContext>()
    .AddScoped<IGetUrlStorage, GetUrlStorage>()
    .AddScoped<ICreateUrlStorage, CreateUrlStorage>()
    .AddScoped<IGuidFactory, GuidFactory>()
    .AddScoped<IMomentProvider, MomentProvider>()
    .AddScoped<ICacheStorage, CacheStorage>()
    .AddScoped<IEventBus, EventBus>()
    .AddScoped<IUrlEncoder, UrlEncoder>()
    .AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NoTrackingDbContext>();
    await dbContext.Database.MigrateAsync(); // TODO: dev mode migrations
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