using Mediator;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;

public record CreateOrUpdateUrlMappingUseCaseRequest(
    UrlCode Code,
    OriginalUrl Original,
    DateTimeOffset CreatedAt)
    : IRequest<CreateOrUpdateUrlMappingUseCaseResponse>;

public record CreateOrUpdateUrlMappingUseCaseResponse(bool Success);

public class CreateOrUpdateUrlMappingUseCase(ICreateOrUpdateUrlMappingStorage storage)
    : IRequestHandler<CreateOrUpdateUrlMappingUseCaseRequest, CreateOrUpdateUrlMappingUseCaseResponse>
{
    public async ValueTask<CreateOrUpdateUrlMappingUseCaseResponse> Handle(
        CreateOrUpdateUrlMappingUseCaseRequest request, CancellationToken cancellationToken)
    {
        var storedUrlMapping = await storage.TryGet(request.Code, CancellationToken.None)
                               ?? new UrlMapping(
                                   request.Code,
                                   request.Original,
                                   request.CreatedAt.ToUniversalTime(),
                                   false,
                                   []);

        var urlMapping = storedUrlMapping with
        {
            Original = request.Original, CreatedAt = request.CreatedAt.ToUniversalTime()
        };

        var savingResult = await storage.Save(urlMapping, cancellationToken);

        return new CreateOrUpdateUrlMappingUseCaseResponse(savingResult);
    }
}

public interface ICreateOrUpdateUrlMappingStorage : IStorage
{
    Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct);
    Task<bool> Save(UrlMapping um, CancellationToken ct);
}