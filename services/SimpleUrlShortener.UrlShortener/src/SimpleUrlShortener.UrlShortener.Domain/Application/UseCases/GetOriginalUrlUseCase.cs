using System.Net;
using Mediator;
using SimpleUrlShortener.UrlShortener.Domain.Core;
using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;

public record GetOriginalUrlRequest(UrlCode Code, GetOriginalUrlRequest.RequestMetadata Metadata) : IRequest<GetOriginalUrlResponse>
{
    public record RequestMetadata(IPAddress IpAddress);
}

public record GetOriginalUrlResponse(OriginalUrl Original);

public class GetOriginalUrlUseCase(
    UrlShortenerSettings settings,
    IShardProvider shardProvider,
    IEventBus eventBus)
    : IRequestHandler<GetOriginalUrlRequest, GetOriginalUrlResponse>
{
    public async ValueTask<GetOriginalUrlResponse> Handle(
        GetOriginalUrlRequest request,
        CancellationToken cancellationToken)
    {
        var urlCode = request.Code;
        var shardId = new ShardId(urlCode.Value[0]);
        var readRepo = shardProvider.GetReadRepository(shardId);

        var urlMapping = await readRepo.GetByCode(urlCode, cancellationToken);
        if (urlMapping is null)
        {
            throw new DomainException(
                $"Cannot find short url {urlCode} with instance prefix {settings.InstancePrefix}");
        }

        var message = new UrlRedirectedMessage(
            settings.InstancePrefix,
            urlMapping.Code.Value,
            urlMapping.Original.Value,
            DateTimeOffset.Now,
            request.Metadata.IpAddress.ToString());
        await eventBus.Publish(message, cancellationToken);

        return new GetOriginalUrlResponse(urlMapping.Original);
    }
}