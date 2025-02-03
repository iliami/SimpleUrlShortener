using MediatR;
using SimpleUrlShortener.Domain.Events;
using SimpleUrlShortener.Domain.Shared;

namespace SimpleUrlShortener.Domain.GetUrlUseCase;

public class GetUrlUseCase(
    IGetUrlStorage storage,
    IEventBus eventBus) 
    : IRequestHandler<GetUrlRequest, Result<GetUrlResponse>>
{
    public async Task<Result<GetUrlResponse>> Handle(GetUrlRequest request, CancellationToken ct = default)
    {
        var originalUrl = await storage.GetUrl(request.UrlCode, ct);
        if (originalUrl is null)
        {
            var error = new Error(
                "Url.NotFound", 
                $"Url not found for the specified code: {request.UrlCode}");
            return Result<GetUrlResponse>.Failure(error);
        }

        var urlClickedEvent = new UrlClickedEvent(originalUrl, request.UrlCode, DateTimeOffset.UtcNow);
        eventBus.Publish(urlClickedEvent, ct).FireAndForget();

        var response = new GetUrlResponse(originalUrl);
        return response;
    }
}