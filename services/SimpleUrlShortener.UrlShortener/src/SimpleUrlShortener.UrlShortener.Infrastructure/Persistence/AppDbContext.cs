using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.Persistence;

public class AppDbContext(
    IConfiguration configuration,
    ILoggerFactory loggerFactory) : DbContext
{
    public DbSet<UrlMappingEntity> UrlMappings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLoggerFactory(loggerFactory)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseNpgsql(configuration.GetConnectionString("Postgres"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UrlMappingEntity>().ToTable("UrlMapping");

        modelBuilder.Entity<UrlMappingEntity>().HasKey(u => u.Code);

        modelBuilder.Entity<UrlMappingEntity>().Property(u => u.Code).HasMaxLength(UrlCode.Length);
        modelBuilder.Entity<UrlMappingEntity>().HasIndex(u => u.Code).HasMethod("hash");

        modelBuilder.Entity<UrlMappingEntity>().Property(u => u.Original).HasMaxLength(OriginalUrl.MaxLength);
        modelBuilder.Entity<UrlMappingEntity>().HasIndex(u => u.Original).HasMethod("hash");
    }
}