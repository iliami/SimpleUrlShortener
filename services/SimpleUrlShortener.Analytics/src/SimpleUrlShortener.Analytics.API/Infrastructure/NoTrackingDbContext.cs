using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.Analytics.Infrastructure.Entities;

namespace SimpleUrlShortener.Analytics.Infrastructure;

public class NoTrackingDbContext(
    IConfiguration configuration, 
    ILoggerFactory loggerFactory) : DbContext
{
    public DbSet<Url> Urls { get; set; }
    public DbSet<UrlCreationMoment> UrlCreationMoments { get; set; }
    public DbSet<UrlClick> UrlClicks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLoggerFactory(loggerFactory)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseNpgsql(configuration.GetConnectionString("Postgres"));
    }
}