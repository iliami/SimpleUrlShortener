using SimpleUrlShortener.Domain;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace SimpleUrlShortener.Infrastructure;

public class UrlEncoder(IReadonlyCache readonlyCache, ILogger<UrlEncoder> logger) : IUrlEncoder
{
    public async Task<string> Encode(string url, CancellationToken ct = default)
    {
        var cachedCode = await readonlyCache.Get<string?>(url, ct);
        if (cachedCode is not null)
        {
            return cachedCode;
        }

        var codeChars = new char[UrlCodeSettings.CodeLength];
        var maxValue = UrlCodeSettings.Alphabet.Length;
        
        var code = string.Empty;
        var attempts = UrlCodeSettings.MaxAttemptsToResolveCollision;
        while (attempts > 0)
        {
            for (var i = 0; i < UrlCodeSettings.CodeLength; i++)
            {
                var randomIndex = RandomNumberGenerator.GetInt32(maxValue);

                codeChars[i] = UrlCodeSettings.Alphabet[randomIndex];
            }

            code = new string(codeChars);

            var cachedUrl = await readonlyCache.Get<string?>(code, ct);
            if (cachedUrl is null || cachedUrl == url)
            {
                break;
            }
            attempts--;
        }

        if (attempts == 0)
        {
            logger.LogError("Failed to encode url: {Url}", url);
            throw new Exception($"Failed to encode url: {url}");
        }

        logger.LogInformation("Result of url {Url} encoding: {UrlCode}", url, code);

        return code;
    }
}