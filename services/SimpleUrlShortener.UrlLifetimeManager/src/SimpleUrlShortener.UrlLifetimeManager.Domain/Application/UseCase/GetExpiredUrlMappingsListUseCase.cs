using Mediator;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;

public record GetExpiredUrlMappingsListUseCaseRequest(int MaxCount)
    : IRequest<GetExpiredUrlMappingsListUseCaseResponse>;

public record GetExpiredUrlMappingsListUseCaseResponse(UrlCode[] UrlCodes);

public class GetExpiredUrlMappingsListUseCase(
    IGetExpiredUrlMappingsListStorage storage)
    : IRequestHandler<GetExpiredUrlMappingsListUseCaseRequest, GetExpiredUrlMappingsListUseCaseResponse>
{
    public async ValueTask<GetExpiredUrlMappingsListUseCaseResponse> Handle(
        GetExpiredUrlMappingsListUseCaseRequest request, CancellationToken cancellationToken)
    {
        if (request.MaxCount is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(request));
        }

        var now = DateTimeOffset.UtcNow;

        var ucs = await storage.GetExpiredUrlMappingsList(
            now, request.MaxCount, cancellationToken);

        return new GetExpiredUrlMappingsListUseCaseResponse(ucs);
    }
}

public interface IGetExpiredUrlMappingsListStorage : IStorage
{
    Task<UrlCode[]> GetExpiredUrlMappingsList(
        DateTimeOffset now,
        int limit,
        CancellationToken cancellationToken);
}