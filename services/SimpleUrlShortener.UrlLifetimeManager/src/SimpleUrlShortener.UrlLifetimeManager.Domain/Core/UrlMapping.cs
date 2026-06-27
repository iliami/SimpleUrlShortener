using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

public record UrlMapping(
    UrlCode Code, 
    OriginalUrl Original, 
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    IImmutableList<UrlMappingRedirection> Redirections);

public readonly record struct UrlCode
{
    public const int MaxLength = 32;
    private const int MaxCodeLength = MaxLength - 1;

    private char InstancePrefix { get; }
    private string Code { get; }
    
    public string Value { get; private init; }

    public UrlCode(
        char instancePrefix,
        [StringLength(UrlCode.MaxCodeLength)]
        string code)
    {
        InstancePrefix = instancePrefix;
        Code = code;
        Value = InstancePrefix + Code;
    }
    
    public UrlCode(
        [StringLength(UrlCode.MaxLength)]
        string value)
    {
        InstancePrefix = '\0';
        Code = string.Empty;
        Value = value;
    }
}

public readonly record struct OriginalUrl(
    [property: Url, StringLength(OriginalUrl.MaxLength)]
    string Value)
{
    public const int MaxLength = 2048;
}