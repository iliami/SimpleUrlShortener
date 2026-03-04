using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Domain.Application;

public interface IUrlShortenerClient
{
    Task Delete(UrlCode urlCode, CancellationToken cancellationToken);
}