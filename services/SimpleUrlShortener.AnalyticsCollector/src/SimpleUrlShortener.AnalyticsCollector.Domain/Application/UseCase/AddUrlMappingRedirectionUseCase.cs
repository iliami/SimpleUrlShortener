using Mediator;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;

public record AddUrlMappingRedirectionUseCaseRequest(
    UrlCode Code,
    DateTimeOffset RedirectedAt,
    Ip Ip)
    : IRequest<AddUrlMappingRedirectionUseCaseResponse>;

public record AddUrlMappingRedirectionUseCaseResponse(bool Success);

public class AddUrlMappingRedirectionUseCase(
    IAddUrlMappingRedirectionStorage storage,
    IGeoIpService geoIpService)
    : IRequestHandler<AddUrlMappingRedirectionUseCaseRequest, AddUrlMappingRedirectionUseCaseResponse>
{
    public async ValueTask<AddUrlMappingRedirectionUseCaseResponse> Handle(
        AddUrlMappingRedirectionUseCaseRequest request,
        CancellationToken cancellationToken)
    {
        var storedUrlMapping = await storage.TryGet(request.Code, cancellationToken)
                               ?? throw new NotFoundException(typeof(UrlMapping));

        var coordinates = await geoIpService.GetCoordinates(request.Ip, cancellationToken);

        var redirection = new UrlMappingRedirection(
            request.RedirectedAt.ToUniversalTime(),
            request.Ip,
            coordinates);

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