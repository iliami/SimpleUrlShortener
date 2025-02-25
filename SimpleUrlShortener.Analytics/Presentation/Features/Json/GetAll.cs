using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.Analytics.Domain;
using SimpleUrlShortener.Analytics.Domain.UseCases.GetAll;
using SimpleUrlShortener.Analytics.Presentation.Identity;

namespace SimpleUrlShortener.Analytics.Presentation.Features.Json;

public class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("json", [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
                async (
                    [AsParameters] GetAllRequest request,
                    [FromServices] IMediator mediator,
                    CancellationToken ct) =>
                {
                    var response = await mediator.Send(request, ct);
                    return response.Match(Results.BadRequest, result => Results.Ok(result.Urls));
                })
            .RequireAuthorization()
            .WithTags(EndpointTags.JsonData)
            .Produces<IEnumerable<Url>>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}