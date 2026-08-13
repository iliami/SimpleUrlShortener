using System.ComponentModel.DataAnnotations;
using Mediator;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;

public record EnrichRedirectionsWithCoordinatesUseCaseRequest([Range(1, 1000)] int BatchSize)
    : IRequest<EnrichRedirectionsWithCoordinatesUseCaseResponse>;

public record EnrichRedirectionsWithCoordinatesUseCaseResponse(
    int TotalProcessed,
    int SuccessfullyEnriched);

public class EnrichRedirectionsWithCoordinatesUseCase(
    IEnrichRedirectionsWithCoordinatesStorage storage,
    IGeoIpService geoIpService)
    : IRequestHandler<EnrichRedirectionsWithCoordinatesUseCaseRequest, EnrichRedirectionsWithCoordinatesUseCaseResponse>
{
    public async ValueTask<EnrichRedirectionsWithCoordinatesUseCaseResponse> Handle(
        EnrichRedirectionsWithCoordinatesUseCaseRequest request,
        CancellationToken cancellationToken)
    {
        var redirections = await storage.GetPublicIpRedirectionsWithoutCoordinates(
            request.BatchSize,
            cancellationToken);

        if (redirections.Length == 0)
        {
            return new EnrichRedirectionsWithCoordinatesUseCaseResponse(0, 0);
        }

        var ipCoordinates = await geoIpService.GetCoordinates(
            redirections.Select(x => x.Ip),
            cancellationToken);

        if (ipCoordinates.Count == 0)
        {
            return new EnrichRedirectionsWithCoordinatesUseCaseResponse(redirections.Length, 0);
        }

        var redirectionsWithCoordinates = new List<UrlMappingRedirectionWithCoordinates>(ipCoordinates.Count);

        foreach (var redirection in redirections)
        {
            if (!ipCoordinates.TryGetValue(redirection.Ip, out var coordinates))
            {
                continue;
            }

            var redirectionWithCoordinates = new UrlMappingRedirectionWithCoordinates(
                redirection.Id,
                redirection.OccuredOn,
                redirection.Ip,
                coordinates
            );

            redirectionsWithCoordinates.Add(redirectionWithCoordinates);
        }

        var succeeded = await storage.Save(redirectionsWithCoordinates.ToArray(), cancellationToken);
        return new EnrichRedirectionsWithCoordinatesUseCaseResponse(redirections.Length, succeeded);
    }
}

public interface IEnrichRedirectionsWithCoordinatesStorage : IStorage
{
    Task<UrlMappingRedirection[]> GetPublicIpRedirectionsWithoutCoordinates(int batchSize, CancellationToken ct);
    Task<int> Save(UrlMappingRedirectionWithCoordinates[] redirections, CancellationToken ct);
}