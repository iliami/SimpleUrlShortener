using MediatR;
using SimpleUrlShortener.Domain.Events;
using SimpleUrlShortener.Domain.Shared;

namespace SimpleUrlShortener.Domain.CreateUrlUseCase;

public class CreateUrlUseCase(
    ICreateUrlStorage storage, 
    IUrlEncoder urlEncoder,
    IEventBus eventBus) 
    : IRequestHandler<CreateUrlRequest, Result<CreateUrlResponse>>
{
    private static readonly string[] Protocols = ["http://", "https://", "ftp://", "ftps://", "file://"];
    public async Task<Result<CreateUrlResponse>> Handle(CreateUrlRequest request, CancellationToken ct = default)
    {
        var isAnyProtocol = Protocols.Any(p => request.Url.StartsWith(p, StringComparison.InvariantCultureIgnoreCase));
        if (!isAnyProtocol)
        {
            request = request with { Url = "https://" + request.Url };
        }
        var urlCode = await urlEncoder.Encode(request.Url, ct);
        var urlDto = new UrlDto(request.Url, urlCode);

        var url = await storage.CreateUrl(urlDto, ct);

        eventBus.Publish(url.ToEventModel(), ct).FireAndForget();

        var response = new CreateUrlResponse(url.Original, url.Code);
        return response;
    }
}