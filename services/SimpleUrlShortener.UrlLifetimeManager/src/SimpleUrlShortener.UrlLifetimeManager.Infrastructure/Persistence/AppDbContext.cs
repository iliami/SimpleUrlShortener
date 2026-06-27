using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence;

public class AppDbContext(
    IConfiguration configuration,
    ILoggerFactory loggerFactory) : DbContext
{
    public DbSet<UrlMappingEntity> UrlMappings { get; set; }
    public DbSet<UrlMappingRedirectionEntity> UrlMappingRedirections { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLoggerFactory(loggerFactory)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseNpgsql(configuration.GetConnectionString("Postgres"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("url-lifetime-manager");

        // UrlMappingEntity
        modelBuilder.Entity<UrlMappingEntity>().ToTable("UrlMapping");

        modelBuilder.Entity<UrlMappingEntity>().HasKey(u => u.Code);

        modelBuilder.Entity<UrlMappingEntity>().Property(u => u.Code).HasMaxLength(UrlCode.MaxLength);

        modelBuilder.Entity<UrlMappingEntity>().Property(u => u.Original).HasMaxLength(OriginalUrl.MaxLength);

        modelBuilder.Entity<UrlMappingEntity>().HasMany(u => u.UrlMappingRedirections)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        // UrlMappingRedirectionEntity
        modelBuilder.Entity<UrlMappingRedirectionEntity>().ToTable("UrlMappingRedirection");
    }
}