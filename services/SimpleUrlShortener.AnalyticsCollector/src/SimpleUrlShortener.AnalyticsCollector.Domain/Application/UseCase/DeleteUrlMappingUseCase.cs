using Mediator;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;

public record DeleteUrlMappingRequest(UrlCode Code) : IRequest;

public class DeleteUrlMappingUseCase(
    IDeleteUrlMappingStorage storage)
    : IRequestHandler<DeleteUrlMappingRequest>
{
    public async ValueTask<Unit> Handle(DeleteUrlMappingRequest request, CancellationToken cancellationToken)
    {
        var urlMapping = await storage.TryGet(request.Code, cancellationToken)
                         ?? throw new NotFoundException<UrlMapping>($"UrlCode: {request.Code.Value}");

        var urlMappingDeletion = urlMapping.Map(DateTimeOffset.UtcNow);
        await storage.Save(urlMappingDeletion, cancellationToken);

        return Unit.Value;
    }
}

public interface IDeleteUrlMappingStorage : IStorage
{
    Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct);
    Task<bool> Save(UrlMappingDeletion urlMappingDeletion, CancellationToken cancellationToken = default);
}