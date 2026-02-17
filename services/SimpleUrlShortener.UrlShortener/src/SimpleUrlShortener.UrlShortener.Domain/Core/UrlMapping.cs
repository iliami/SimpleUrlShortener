using System.ComponentModel.DataAnnotations;

namespace SimpleUrlShortener.UrlShortener.Domain.Core;

public record UrlMapping(UrlCode Code, OriginalUrl Original);

public readonly record struct UrlCode(
    [property: Length(UrlCode.Length, UrlCode.Length)]
    string Value)
{
    public const int Length = UrlShortenerSettings.TotalCodeLength - 1;
}

public readonly record struct OriginalUrl([property: Url, StringLength(OriginalUrl.MaxLength)] string Value)
{
    public const int MaxLength = 2048;
}