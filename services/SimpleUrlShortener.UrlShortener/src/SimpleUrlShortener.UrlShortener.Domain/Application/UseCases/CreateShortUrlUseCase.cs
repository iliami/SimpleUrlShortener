using Mediator;
using SimpleUrlShortener.UrlShortener.Domain.Core;
using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;

public record CreateShortUrlRequest(OriginalUrl Original) : IRequest<CreateShortUrlResponse>;

public record CreateShortUrlResponse(char CodePrefix, UrlCode Code);

public class CreateShortUrlUseCase(
    UrlShortenerSettings settings,
    IUrlEncoder urlEncoder,
    IShardProvider shardProvider,
    IEventBus eventBus)
    : IRequestHandler<CreateShortUrlRequest, CreateShortUrlResponse>
{
    public async ValueTask<CreateShortUrlResponse> Handle(
        CreateShortUrlRequest request,
        CancellationToken cancellationToken)
    {
        var originalUrl = request.Original;
        var code = string.Empty;
        UrlMapping? storedCode = null;
        var attempts = Math.Max(1, settings.MaxAttemptsToResolveCollision);
        while (attempts-- > 0)
        {
            code = urlEncoder.Encode(
                originalUrl,
                UrlCode.Length,
                UrlShortenerSettings.CodeAlphabet);
            var codeShardId = new ShardId(code[0]);
            var codeReadRepo = shardProvider.GetReadRepository(codeShardId);
            storedCode = await codeReadRepo.GetByCode(
                new UrlCode(code), cancellationToken);
            if (storedCode is null)
            {
                break;
            }
        }

        if (attempts == 0 && storedCode is null)
        {
            throw new DomainException(
                $"Cannot create short url for {request.Original} with instance prefix {settings.InstancePrefix}");
        }

        var urlCode = new UrlCode(code);
        var urlMapping = new UrlMapping(urlCode, originalUrl);

        var shardId = new ShardId(code[0]);
        var writeRepo = shardProvider.GetWriteRepository(shardId);
        var writingResult = await writeRepo.Save(urlMapping, cancellationToken);

        if (!writingResult)
        {
            throw new DomainException(
                $"Cannot create short url {urlCode} with instance prefix {settings.InstancePrefix}");
        }

        var message = new UrlCreatedMessage(
            settings.InstancePrefix,
            urlMapping.Code.Value,
            urlMapping.Original.Value,
            DateTimeOffset.Now);
        await eventBus.Publish(message, cancellationToken);
        return new CreateShortUrlResponse(settings.InstancePrefix, urlCode);
    }
}