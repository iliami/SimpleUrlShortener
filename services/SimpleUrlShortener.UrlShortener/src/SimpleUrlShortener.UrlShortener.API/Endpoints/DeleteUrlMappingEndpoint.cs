using Mediator;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;
using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.API.Endpoints;

public class DeleteUrlMappingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{urlCode:required}", Handle)
            .AddEndpointFilter<ApiAuthorizationEndpointFilter>();
    }

    private static async Task<IResult> Handle(
        [FromRoute] string urlCode,
        [FromServices] IMediator mediator,
        [FromServices] ILogger<GetOriginalUrlEndpoint> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new DeleteUrlMappingRequest(new UrlCode(urlCode[1..]));
            await mediator.Send(request, cancellationToken);
            return TypedResults.Ok();
        }
        catch (NotFoundException<UrlMapping>)
        {
            return TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError("Exception at DeleteUrlMappingEndpoint {Exception}", ex);
            return TypedResults.InternalServerError();
        }
    }
}