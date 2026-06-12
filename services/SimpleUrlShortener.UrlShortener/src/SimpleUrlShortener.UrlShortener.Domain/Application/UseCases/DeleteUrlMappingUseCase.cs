using Mediator;
using SimpleUrlShortener.UrlShortener.Domain.Core;
using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;

public record DeleteUrlMappingRequest(UrlCode Code) : IRequest;

public class DeleteUrlMappingUseCase(
    UrlShortenerSettings settings,
    IShardProvider shardProvider,
    IEventBus eventBus)
    : IRequestHandler<DeleteUrlMappingRequest>
{
    public async ValueTask<Unit> Handle(DeleteUrlMappingRequest request, CancellationToken cancellationToken)
    {
        var urlCode = request.Code;
        var shardId = new ShardId(urlCode.Value[0]);
        var readRepo = shardProvider.GetReadRepository(shardId);

        var urlMapping = await readRepo.GetByCode(urlCode, cancellationToken)
                         ?? throw new DomainException(
                             $"Cannot find short url {urlCode} with instance prefix {settings.InstancePrefix}",
                             DomainExceptionCode.NotFound);

        var writeRepo = shardProvider.GetWriteRepository(shardId);
        var deleteResult = await writeRepo.Delete(urlMapping, cancellationToken);
        if (deleteResult)
        {
            var message = new UrlDeletedMessage(settings.InstancePrefix, urlCode.Value);
            await eventBus.Publish(message, cancellationToken);
        }

        return Unit.Value;
    }
}