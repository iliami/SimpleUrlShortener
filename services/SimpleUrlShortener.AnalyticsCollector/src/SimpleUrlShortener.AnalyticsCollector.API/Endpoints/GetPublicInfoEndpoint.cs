using Mediator;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.API.Endpoints;

public class GetPublicInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{urlCode}", Handle);
    }

    private static async Task<IResult> Handle(
        [FromRoute] string urlCode,
        [FromServices] IMediator mediator,
        [FromServices] ILogger<GetPublicInfoEndpoint> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var code = new UrlCode(urlCode);
            var request = new GetPublicInfoUseCaseRequest(code);
            var response = await mediator.Send(request, cancellationToken);
            return TypedResults.Ok(response);
        }
        catch (NotFoundException<UrlMapping>)
        {
            return TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError("Exception at GetLifetimeInfoEndpoint {Exception}", ex);
            return TypedResults.InternalServerError();
        }
    }
}