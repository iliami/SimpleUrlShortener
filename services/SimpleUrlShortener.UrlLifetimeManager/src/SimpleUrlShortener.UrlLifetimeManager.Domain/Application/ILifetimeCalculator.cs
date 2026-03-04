using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Domain.Application;

public interface ILifetimeCalculator
{
    ValueTask<int> CalculateDaysToLive(UrlMapping um, CancellationToken cancellationToken);
}