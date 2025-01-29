using FluentValidation;
using MediatR;

namespace SimpleUrlShortener.Domain.CreateUrlUseCase;

public class CreateUrlUseCase(
    ICreateUrlStorage storage, 
    IValidator<CreateUrlRequest> validator,
    IUrlEncoder urlEncoder,
    IEventBus eventBus) 
    : IRequestHandler<CreateUrlRequest, Result<CreateUrlResponse>>
{
    public async Task<Result<CreateUrlResponse>> Handle(CreateUrlRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.AsResult<CreateUrlResponse>();
        }

        var shortUrl = await urlEncoder.Encode(request.Url);
        var urlDto = new UrlDto(request.Url, shortUrl);

        var url = await storage.CreateUrl(urlDto, ct);

        await eventBus.Publish(url.ToEventModel(), ct);

        var response = new CreateUrlResponse(url.Original, url.Code);
        return response;
    }
}