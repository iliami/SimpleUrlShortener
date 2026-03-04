using Mediator;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.API.Endpoints;

public class GetLifetimeInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{urlCode}/lifetime", Handle);
    }

    private static async Task<IResult> Handle(
        [FromRoute] string urlCode,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var code = new UrlCode(urlCode);
        var request = new GetLifetimeInfoUseCaseRequest(code);
        var response = await mediator.Send(request, cancellationToken);
        return Results.Ok(response);
    }
}