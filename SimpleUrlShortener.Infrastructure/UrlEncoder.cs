using System.Text;
using SimpleUrlShortener.Domain;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace SimpleUrlShortener.Infrastructure;

public class UrlEncoder(IReadonlyCache readonlyCache, ILogger<UrlEncoder> logger) : IUrlEncoder
{
    private const string Alphabet = "qwertyuiopasdfghjklzxcvbnm.,?";
    // private static readonly RandomNumberGenerator RandomNumberGenerator = RandomNumberGenerator.Create();
    public async Task<string> Encode(string url, CancellationToken ct = default)
    {
        var alphabetLength = (uint)Alphabet.Length;

        var source = Encoding.UTF8.GetBytes(url);
        var encodedString = new StringBuilder();
        var code = string.Empty;

        var attempts = 50;
        while (attempts > 0) 
        {
            // url to num via hash func
            var hashBytes = MD5.HashData(source);
            var num = BitConverter.ToUInt64(hashBytes, 0);

            // num to code with alphabet
            if (num == 0)
            {
                return Alphabet[0].ToString();
            }
            
            while (num > 0)
            {
                var remainder = num % alphabetLength;
                encodedString.Append(Alphabet[(int)remainder]);
                num /= alphabetLength;
            }
            code = encodedString.ToString();

            // check collision
            var cachedUrl = await readonlyCache.Get<string?>(code, ct);
            if (cachedUrl is null || cachedUrl == url)
            {
                logger.LogInformation("CachedUrl: {CachedUrl} Url: {Url} UrlCode: {UrlCode}", cachedUrl, url, code);
                break;
            }
            encodedString.Clear();
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