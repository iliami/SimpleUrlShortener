using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Domain.Application;

public interface IUrlReadRepository
{
    Task<UrlMapping?> GetByCode(in UrlCode urlCode, CancellationToken cancellationToken = default);
}