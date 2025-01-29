using System.Text;
using SimpleUrlShortener.Domain;
using System.Security.Cryptography;

namespace SimpleUrlShortener.Infrastructure;

public class UrlEncoder(IReadonlyCache readonlyCache) : IUrlEncoder
{
    private const string Alphabet = "qwertyuiopasdfghjklzxcvbnm.,?";

    public async Task<string> Encode(string url)
    {
        var source = Encoding.UTF8.GetBytes(url);
        var encodedString = new StringBuilder();
        var code = string.Empty;

        var attempts = 50;
        while (attempts > 0) 
        {
            // url to num via hash func
            var hashBytes = MD5.HashData(source);
            var num = BitConverter.ToInt64(hashBytes, 0);

            // num to code with alphabet
            if (num == 0)
            {
                return Alphabet[0].ToString();
            }

            while (num > 0)
            {
                var remainder = num % Alphabet.Length;
                encodedString.Append(Alphabet[(int)remainder]);
                num /= Alphabet.Length;
            }
            code = encodedString.ToString();

            // check collision
            var cachedUrl = await readonlyCache.Get<string?>(code);
            if (cachedUrl is null || cachedUrl == url)
            {
                break;
            }
            encodedString.Clear();
            attempts--;
        }

        if (attempts == 0)
        {
            throw new Exception($"Failed to encode url: {url}");
        }
        return code;
    }
}