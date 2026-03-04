using Mediator;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;

public record UpdateUrlMappingLifetimeUseCaseRequest(UrlCode UrlCode) : IRequest;

public class UpdateUrlMappingLifetimeUseCase(
    IUpdateUrlMappingLifetimeStorage storage,
    ILifetimeCalculator lifetimeCalculator,
    IUrlShortenerClient urlShortenerClient)
    : IRequestHandler<UpdateUrlMappingLifetimeUseCaseRequest>
{
    public async ValueTask<Unit> Handle(UpdateUrlMappingLifetimeUseCaseRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var um = await storage.TryGet(request.UrlCode, cancellationToken)
                 ?? throw new NotFoundException(typeof(UrlMapping));

        if (um.ExpiresAt - now > TimeSpan.FromMinutes(10))
        {
            throw new DomainException("Url mapping is not expired yet");
        }

        var daysToLive = await lifetimeCalculator.CalculateDaysToLive(um, cancellationToken);

        if (daysToLive <= 0)
        {
            await urlShortenerClient.Delete(request.UrlCode, cancellationToken);
            var deleteResult = await storage.Delete(request.UrlCode, cancellationToken);
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
}

public interface IUpdateUrlMappingLifetimeStorage : IStorage
{
    Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct);
    Task<bool> Delete(UrlCode code, CancellationToken ct);
    Task<bool> Save(UrlMapping um, CancellationToken ct);
}