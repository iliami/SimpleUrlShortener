using Mediator;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;

public record GetPublicInfoUseCaseRequest(
    UrlCode Code)
    : IRequest<GetPublicInfoUseCaseResponse>;

public record GetPublicInfoUseCaseResponse(
    long TotalRedirectionCount,
    DateTimeOffset LastRedirectionDate);

public class GetPublicInfoUseCase(
    IGetPublicInfoStorage storage)
    : IRequestHandler<GetPublicInfoUseCaseRequest, GetPublicInfoUseCaseResponse>
{
    public async ValueTask<GetPublicInfoUseCaseResponse> Handle(
        GetPublicInfoUseCaseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await storage.GetInfo(
            request.Code,
            cancellationToken);

        return new GetPublicInfoUseCaseResponse(
            result.TotalRedirectionCount,
            result.LastRedirectionDate);
    }
}

public interface IGetPublicInfoStorage : IStorage
{
    Task<(long TotalRedirectionCount, DateTimeOffset LastRedirectionDate)> GetInfo(
        UrlCode urlCode, CancellationToken cancellationToken);
}