using Mediator;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;

public record AddUrlMappingRedirectionUseCaseRequest(
    UrlCode Code,
    DateTimeOffset RedirectedAt)
    : IRequest<AddUrlMappingRedirectionUseCaseResponse>;

public record AddUrlMappingRedirectionUseCaseResponse(bool Success);

public class AddUrlMappingRedirectionUseCase(
    IAddUrlMappingRedirectionStorage storage)
    : IRequestHandler<AddUrlMappingRedirectionUseCaseRequest, AddUrlMappingRedirectionUseCaseResponse>
{
    public async ValueTask<AddUrlMappingRedirectionUseCaseResponse> Handle(
        AddUrlMappingRedirectionUseCaseRequest request,
        CancellationToken cancellationToken)
    {
        var storedUrlMapping = await storage.TryGet(request.Code, cancellationToken)
                               ?? throw new NotFoundException<UrlMapping>($"UrlCode: {request.Code.Value}");

        var redirection = new UrlMappingRedirection(
            request.RedirectedAt.ToUniversalTime());

        var urlMapping = storedUrlMapping with
        {
            Redirections = storedUrlMapping.Redirections.Add(redirection)
        };

        var savingResult = await storage.Save(urlMapping, cancellationToken);

        return new AddUrlMappingRedirectionUseCaseResponse(savingResult);
    }
}

public interface IAddUrlMappingRedirectionStorage : IStorage
{
    Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct);
    Task<bool> Save(UrlMapping um, CancellationToken ct);
}