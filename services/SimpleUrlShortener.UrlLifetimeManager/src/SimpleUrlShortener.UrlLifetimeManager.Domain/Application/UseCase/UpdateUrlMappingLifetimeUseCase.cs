using Mediator;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;

public record UpdateUrlMappingLifetimeUseCaseRequest(UrlCode Code) : IRequest;

public class UpdateUrlMappingLifetimeUseCase(
    IUpdateUrlMappingLifetimeStorage storage,
    IUrlShortenerClient urlShortenerClient)
    : IRequestHandler<UpdateUrlMappingLifetimeUseCaseRequest>
{
    private const int MaxIdleDays = 30;
    private const int MaxTotalDays = 90;

    public async ValueTask<Unit> Handle(UpdateUrlMappingLifetimeUseCaseRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var um = await storage.TryGet(request.Code, cancellationToken)
                 ?? throw new NotFoundException<UrlMapping>($"UrlCode: {request.Code.Value}");

        if (um.ExpiresAt - now > TimeSpan.FromMinutes(10))
        {
            throw new DomainException("Url mapping is not expired yet");
        }

        var daysToLive = CalculateDaysToLive(um, now);

        if (daysToLive <= 0)
        {
            await urlShortenerClient.Delete(request.Code, cancellationToken);
            var deleteResult = await storage.Delete(request.Code, cancellationToken);
            return deleteResult
                ? Unit.Value
                : throw new DomainException("Url mapping is not deleted");
        }

        um = um with
        {
            ExpiresAt = um.ExpiresAt + TimeSpan.FromDays(daysToLive)
        };

        var savingResult = await storage.Save(um, cancellationToken);
        return savingResult
            ? Unit.Value
            : throw new DomainException("Url mapping is not saved");
    }
    
    private static int CalculateDaysToLive(UrlMapping um, DateTimeOffset now)
    {
        // Дни с создания ссылки
        var ageDays = (now - um.CreatedAt).TotalDays;

        if (ageDays >= MaxTotalDays)
        {
            return 0;
        }

        if (um.Redirections.Count == 0)
        {
            var remaining = MaxIdleDays - ageDays;
            return remaining > 0 
                ? (int)remaining 
                : 0;
        }

        var lastRedirection = um.Redirections
            .MaxBy(x => x.OccuredOn)!.OccuredOn;

        // Дни с последнего перехода
        var idleDays = (now - lastRedirection).TotalDays;

        if (idleDays >= MaxIdleDays)
        {
            return 0;
        }

        var remainingUntilIdle = MaxIdleDays - idleDays;

        var remainingUntilAbsolute = MaxTotalDays - ageDays;

        var remainingDays = Math.Min(remainingUntilIdle, remainingUntilAbsolute);
        remainingDays = Math.Max(remainingDays, 0);

        return (int)remainingDays;
    }
}

public interface IUpdateUrlMappingLifetimeStorage : IStorage
{
    Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct);
    Task<bool> Delete(UrlCode code, CancellationToken ct);
    Task<bool> Save(UrlMapping um, CancellationToken ct);
}