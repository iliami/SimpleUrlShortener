using SimpleUrlShortener.UrlShortener.Domain;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SimpleUrlShortener.UrlShortener.Infrastructure;

public class UrlEncoder(ICacheStorage storage, NoTrackingDbContext dbContext, ILogger<UrlEncoder> logger) : IUrlEncoder
{
    public async Task<string> Encode(string originalUrl, CancellationToken ct = default)
    {
        var cachedCode =
            await storage.Get<string>(originalUrl, ct);
        if (cachedCode is not null)
        {
            return cachedCode;
        }

        var storedUrl = await dbContext.Urls
            .FirstOrDefaultAsync(u => u.Original == originalUrl, ct);
        if (storedUrl is not null)
        {
            await storage.Set(originalUrl, storedUrl.Code, TimeSpan.FromHours(12), ct);
            return storedUrl.Code;
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

            var cachedUrl = await storage.Get<Url>(code, ct);
            if (cachedUrl is null)
            {
                break;
            }
            attempts--;
        }

        if (attempts == 0)
        {
            logger.LogError("Failed to encode url: {Url}", originalUrl);
            throw new Exception($"Failed to encode url: {originalUrl}");
        }

        logger.LogInformation("Result of url {Url} encoding: {UrlCode}", originalUrl, code);

        return code;
    }
}