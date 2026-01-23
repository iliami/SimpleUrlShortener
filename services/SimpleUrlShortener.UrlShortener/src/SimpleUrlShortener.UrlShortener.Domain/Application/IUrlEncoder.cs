using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Domain.Application;

public interface IUrlEncoder
{
    string Encode(
        in OriginalUrl originalUrl,
        in int codeLength,
        in ReadOnlySpan<char> codeAlphabet);
}