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

        urlMapping = urlMapping with { IsRevoked = true };
        await storage.Save(urlMapping, cancellationToken);

        return Unit.Value;
    }
}

public interface IDeleteUrlMappingStorage : IStorage
{
    Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct);
    Task<bool> Save(UrlMapping urlMapping, CancellationToken cancellationToken = default);
}