using Mediator;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.API.Endpoints;

public class GetPublicInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{urlCode}/info", Handle);
    }

    private static async Task<IResult> Handle(
        [FromRoute] string urlCode,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var code = new UrlCode(urlCode);
        var request = new GetPublicInfoUseCaseRequest(code);
        var response = await mediator.Send(request, cancellationToken);
        return Results.Ok(new { response.TotalRedirectionCount, response.LastRedirectionDate });
    }
}