using MediatR;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.UrlShortener.Domain.Events;
using SimpleUrlShortener.UrlShortener.Domain.Shared;

namespace SimpleUrlShortener.UrlShortener.Domain.GetUrlUseCase;

public class GetUrlUseCase(
    IGetUrlStorage storage,
    IEventBus eventBus) 
    : IRequestHandler<GetUrlRequest, Result<GetUrlResponse>>
{
    public async Task<Result<GetUrlResponse>> Handle(GetUrlRequest request, CancellationToken ct = default)
    {
        var url = await storage.GetUrl(request.UrlCode, ct);
        if (url is null)
        {
            var error = new Error(
                "Url.NotFound", 
                $"Url not found for the specified code: {request.UrlCode}");
            return Result<GetUrlResponse>.Failure(error);
        }

        eventBus.Publish(url.ToEventModel(), ct).FireAndForget();

        var response = new GetUrlResponse(url.Original, url.Code);
        return response;
    }
}