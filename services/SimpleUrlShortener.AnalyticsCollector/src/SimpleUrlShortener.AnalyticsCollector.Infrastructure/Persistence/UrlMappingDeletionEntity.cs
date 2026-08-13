using System.Collections.Immutable;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence;

public class UrlMappingDeletionEntity
{
    public Guid Id { get; set; }
    public string Code { get; init; } = string.Empty;
    public string Original { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset DeletedAt { get; init; }
    public ICollection<UrlMappingRedirectionEntity> UrlMappingRedirections { get; set; } = [];
}

public static class UrlMappingDeletionEntityExtensions
{
    public static UrlMappingDeletionEntity Map(this UrlMappingDeletion um)
        => new()
        {
            Code = um.Code.Value,
            Original = um.Original.Value,
            CreatedAt = um.CreatedAt.ToUniversalTime(),
            DeletedAt = um.DeletedAt.ToUniversalTime(),
            UrlMappingRedirections = um.Redirections.Select(x => x.Map()).ToList()
        };

    public static UrlMappingDeletion Map(this UrlMappingDeletionEntity entity)
        => new(new UrlCode(entity.Code),
            new OriginalUrl(entity.Original),
            entity.CreatedAt.ToUniversalTime(),
            entity.DeletedAt.ToUniversalTime(),
            entity.UrlMappingRedirections.Select(x => x.Map()).ToImmutableList());

    public static async Task<UrlMappingDeletion?> Map(this Task<UrlMappingDeletionEntity?> task)
    {
        var entity = await task;
        return entity?.Map();
    }
}