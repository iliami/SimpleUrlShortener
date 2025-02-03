using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.Infrastructure.Entities;

namespace SimpleUrlShortener.Infrastructure;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Url>().HasKey(u => u.Id);

        modelBuilder.Entity<Url>().Property(u => u.Code).HasMaxLength(Domain.Url.CodeMaxLength);
        modelBuilder.Entity<Url>().HasIndex(u => u.Code).HasMethod("hash");

        modelBuilder.Entity<Url>().Property(u => u.Code).HasMaxLength(Domain.Url.OriginalMaxLength);
        modelBuilder.Entity<Url>().HasIndex(u => u.Original).HasMethod("hash");
    }
}