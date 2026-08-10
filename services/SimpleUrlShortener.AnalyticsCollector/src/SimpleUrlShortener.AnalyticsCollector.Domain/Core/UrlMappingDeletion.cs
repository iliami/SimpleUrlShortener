using System.Collections.Immutable;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Core;

public record UrlMappingDeletion(
    UrlCode Code,
    OriginalUrl Original,
    DateTimeOffset CreatedAt,
    DateTimeOffset DeletedAt,
    IImmutableList<UrlMappingRedirection> Redirections);

internal static class UrlMappingDeletionExtensions
{
    extension(UrlMapping um)
    {
        internal UrlMappingDeletion Map(DateTimeOffset deletedAt)
            => deletedAt < um.CreatedAt
                ? throw new ArgumentException("Deletion time cannot be before creation time", nameof(deletedAt))
                : new UrlMappingDeletion(um.Code, um.Original, um.CreatedAt, deletedAt, um.Redirections);
    }
}