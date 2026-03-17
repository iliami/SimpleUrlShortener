using Mediator;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;
using SimpleUrlShortener.AnalyticsCollector.Domain.Shared;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;

public record DeleteUrlMappingRequest(UrlCode Code) : IRequest;

public class DeleteUrlMappingUseCase(
    IDeleteUrlMappingStorage storage)
    : IRequestHandler<DeleteUrlMappingRequest>
{
    public async ValueTask<Unit> Handle(DeleteUrlMappingRequest request, CancellationToken cancellationToken)
    {
        var urlMapping = await storage.TryGet(request.Code, cancellationToken)
                         ?? throw new NotFoundException(typeof(UrlMapping),
                             new Dictionary<string, string>
                             {
                                 [nameof(UrlMapping.Code)] = request.Code.Value
                             });

        await storage.Delete(urlMapping, cancellationToken);

        return Unit.Value;
    }
}

public interface IDeleteUrlMappingStorage : IStorage
{
    Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct);
    Task<bool> Delete(UrlMapping urlMapping, CancellationToken cancellationToken = default);
}