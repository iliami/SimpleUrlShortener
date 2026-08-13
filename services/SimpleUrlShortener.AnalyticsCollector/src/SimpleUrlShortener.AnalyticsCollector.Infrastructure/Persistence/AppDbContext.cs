using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence;

public class AppDbContext(
    IConfiguration configuration,
    ILoggerFactory loggerFactory) : DbContext
{
    public DbSet<UrlMappingEntity> UrlMappings { get; set; }
    public DbSet<UrlMappingDeletionEntity> UrlMappingDeletions { get; set; }
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
        modelBuilder.HasDefaultSchema("analytics-collector");

        // UrlMappingEntity
        modelBuilder.Entity<UrlMappingEntity>().ToTable("UrlMapping");

        modelBuilder.Entity<UrlMappingEntity>().HasKey(u => u.Code);

        modelBuilder.Entity<UrlMappingEntity>().Property(u => u.Code).HasMaxLength(UrlCode.MaxLength);

        modelBuilder.Entity<UrlMappingEntity>().Property(u => u.Original).HasMaxLength(OriginalUrl.MaxLength);

        modelBuilder.Entity<UrlMappingEntity>().HasMany(u => u.UrlMappingRedirections)
            .WithOne()
            .HasForeignKey(u => u.UrlMappingId)
            .OnDelete(DeleteBehavior.SetNull);

        // UrlMappingDeletionEntity
        modelBuilder.Entity<UrlMappingDeletionEntity>().ToTable("UrlMappingDeletion");

        modelBuilder.Entity<UrlMappingDeletionEntity>().HasKey(u => u.Id);

        modelBuilder.Entity<UrlMappingDeletionEntity>().Property(u => u.Code).HasMaxLength(UrlCode.MaxLength);

        modelBuilder.Entity<UrlMappingDeletionEntity>().Property(u => u.Original).HasMaxLength(OriginalUrl.MaxLength);

        modelBuilder.Entity<UrlMappingDeletionEntity>().HasMany(u => u.UrlMappingRedirections)
            .WithOne()
            .HasForeignKey(x => x.UrlMappingDeletionId)
            .OnDelete(DeleteBehavior.Cascade);

        // UrlMappingRedirectionEntity
        modelBuilder.Entity<UrlMappingRedirectionEntity>().ToTable("UrlMappingRedirection");

        modelBuilder.Entity<UrlMappingRedirectionEntity>().Property(u => u.UrlMappingId)
            .HasMaxLength(UrlCode.MaxLength);
        modelBuilder.Entity<UrlMappingRedirectionEntity>().Property(u => u.Ip).HasMaxLength(512);
        modelBuilder.Entity<UrlMappingRedirectionEntity>().Property(e => e.IpKind).HasMaxLength(64);
    }
}