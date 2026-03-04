using Mediator;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;

public record GetLifetimeInfoUseCaseRequest(
    UrlCode Code)
    : IRequest<GetLifetimeInfoUseCaseResponse>;

public record GetLifetimeInfoUseCaseResponse(
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

public class GetLifetimeInfoUseCase(
    IGetLifetimeInfoStorage storage)
    : IRequestHandler<GetLifetimeInfoUseCaseRequest, GetLifetimeInfoUseCaseResponse>
{
    public async ValueTask<GetLifetimeInfoUseCaseResponse> Handle(
        GetLifetimeInfoUseCaseRequest request,
        CancellationToken cancellationToken)
    {
        var um = await storage.TryGet(request.Code, cancellationToken)
                 ?? throw new NotFoundException(typeof(UrlMapping));

        return new GetLifetimeInfoUseCaseResponse(um.CreatedAt, um.ExpiresAt);
    }
}

public interface IGetLifetimeInfoStorage : IStorage
{
    Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct);
}