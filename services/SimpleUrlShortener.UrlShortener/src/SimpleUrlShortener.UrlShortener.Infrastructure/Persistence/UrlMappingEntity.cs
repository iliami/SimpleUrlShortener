using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.Persistence;

public class UrlMappingEntity
{
    public string Code { get; init; } = string.Empty;
    public string Original { get; init; } = string.Empty;
}

public static class UrlMappingEntityMapExtensions
{
    public static UrlMappingEntity Map(this UrlMapping um)
        => new()
        {
            Code = um.Code.Value,
            Original = um.Original.Value
        };

    public static UrlMapping Map(this UrlMappingEntity entity)
        => new(new UrlCode(entity.Code), new OriginalUrl(entity.Original));

    public static async Task<UrlMapping?> Map(this Task<UrlMappingEntity?> task)
    {
        var entity = await task;
        return entity?.Map();
    }
}