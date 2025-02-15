using MediatR;
using SimpleUrlShortener.UrlShortener.Domain.Events;
using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Domain.CreateUrlUseCase;

public class CreateUrlUseCase(
    ICreateUrlStorage storage, 
    IUrlEncoder urlEncoder) 
    : IRequestHandler<CreateUrlRequest, Result<CreateUrlResponse>>
{
    private static readonly string[] Protocols = ["http://", "https://"];
    public async Task<Result<CreateUrlResponse>> Handle(CreateUrlRequest request, CancellationToken ct = default)
    {
        var isAnyProtocol = Protocols.Any(p => request.Url.StartsWith(p, StringComparison.InvariantCultureIgnoreCase));
        if (!isAnyProtocol)
        {
            request = request with { Url = "https://" + request.Url };
        }
        var urlCode = await urlEncoder.Encode(request.Url, ct);

        var url = await storage.CreateUrl(request.Url, urlCode, ct);

        var response = new CreateUrlResponse(url.Original, url.Code);
        return response;
    }
}