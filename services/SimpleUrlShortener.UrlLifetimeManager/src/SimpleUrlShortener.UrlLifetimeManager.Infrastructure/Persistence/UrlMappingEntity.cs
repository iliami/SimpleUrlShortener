using System.Collections.Immutable;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence;

public class UrlMappingEntity
{
    public string Code { get; init; } = string.Empty;
    public string Original { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public ICollection<UrlMappingRedirectionEntity> UrlMappingRedirections { get; set; } = [];
}

public static class UrlMappingEntityMapExtensions
{
    public static UrlMappingEntity Map(this UrlMapping um)
        => new()
        {
            Code = um.Code.Value,
            Original = um.Original.Value,
            CreatedAt = um.CreatedAt.ToUniversalTime(),
            ExpiresAt = um.ExpiresAt.ToUniversalTime(),
            UrlMappingRedirections = um.Redirections.Select(x => x.Map()).ToList()
        };

    public static UrlMapping Map(this UrlMappingEntity entity)
        => new(
            new UrlCode(entity.Code),
            new OriginalUrl(entity.Original),
            entity.CreatedAt.ToUniversalTime(),
            entity.ExpiresAt.ToUniversalTime(),
            entity.UrlMappingRedirections.Select(x => x.Map()).ToImmutableList());

    public static async Task<UrlMapping?> Map(this Task<UrlMappingEntity?> task)
    {
        var entity = await task;
        return entity?.Map();
    }
}