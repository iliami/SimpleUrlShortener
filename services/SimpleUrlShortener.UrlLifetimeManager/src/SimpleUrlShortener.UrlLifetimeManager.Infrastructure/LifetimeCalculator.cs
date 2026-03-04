using SimpleUrlShortener.UrlLifetimeManager.Domain.Application;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure;

public class LifetimeCalculator : ILifetimeCalculator
{
    private const int MaxIdleDays = 30;
    private const int MaxTotalDays = 90;

    public ValueTask<int> CalculateDaysToLive(UrlMapping um, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        // Дни с создания ссылки
        var ageDays = (now - um.CreatedAt).TotalDays;

        if (ageDays >= MaxTotalDays)
        {
            return ValueTask.FromResult(0);
        }

        if (um.Redirections.Count == 0)
        {
            var remaining = MaxIdleDays - ageDays;
            return remaining > 0 
                ? ValueTask.FromResult((int)remaining) 
                : ValueTask.FromResult(0);
        }

        var lastRedirection = um.Redirections
            .MaxBy(x => x.OccuredOn)!.OccuredOn;

        // Дни с последнего перехода
        var idleDays = (now - lastRedirection).TotalDays;

        if (idleDays >= MaxIdleDays)
        {
            return ValueTask.FromResult(0);
        }

        var remainingUntilIdle = MaxIdleDays - idleDays;

        var remainingUntilAbsolute = MaxTotalDays - ageDays;

        var remainingDays = Math.Min(remainingUntilIdle, remainingUntilAbsolute);
        remainingDays = Math.Max(remainingDays, 0);

        return ValueTask.FromResult((int)remainingDays);
    }
}