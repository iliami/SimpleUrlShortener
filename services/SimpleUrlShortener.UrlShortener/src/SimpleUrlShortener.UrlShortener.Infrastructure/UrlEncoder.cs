using System.Security.Cryptography;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Infrastructure;

public class UrlEncoder : IUrlEncoder
{
    public string Encode(
        in OriginalUrl originalUrl,
        in int codeLength,
        in ReadOnlySpan<char> codeAlphabet)
        => RandomNumberGenerator.GetString(codeAlphabet, codeLength);
}