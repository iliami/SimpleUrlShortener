using MediatR;
using SimpleUrlShortener.Domain.Shared;

namespace SimpleUrlShortener.Domain.CreateUrlUseCase;

public class CreateUrlUseCase(
    ICreateUrlStorage storage, 
    IUrlNormalizer urlNormalizer,
    IUrlEncoder urlEncoder,
    IEventBus eventBus) 
    : IRequestHandler<CreateUrlRequest, Result<CreateUrlResponse>>
{
    public async Task<Result<CreateUrlResponse>> Handle(CreateUrlRequest request, CancellationToken ct = default)
    {
        var normalizedUrl = await urlNormalizer.NormalizeUrl(request.Url, ct);
        var urlCode = await urlEncoder.Encode(normalizedUrl, ct);
        var urlDto = new UrlDto(request.Url, normalizedUrl, urlCode);

        var url = await storage.CreateUrl(urlDto, ct);

        eventBus.Publish(url.ToEventModel(), ct).FireAndForget();

        var response = new CreateUrlResponse(url.Original, url.Code);
        return response;
    }
}