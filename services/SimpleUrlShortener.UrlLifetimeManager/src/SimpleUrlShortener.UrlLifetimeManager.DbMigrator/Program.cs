using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence;

Console.WriteLine("1. Initializing host...");
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddDbContext<AppDbContext>();
    })
    .Build();
Console.WriteLine("2. Host initialized successfully.");

Console.WriteLine("3. Creating AppDbContext...");
using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
Console.WriteLine("4. AppDbContext created successfully.");

Console.WriteLine("5. Applying migrations...");
db.Database.Migrate();
Console.WriteLine("6. Migrations applied successfully.");
