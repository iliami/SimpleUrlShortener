using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Domain.Application;

public interface IUrlWriteRepository
{
    Task<bool> Save(UrlMapping urlMapping, CancellationToken cancellationToken = default);
}