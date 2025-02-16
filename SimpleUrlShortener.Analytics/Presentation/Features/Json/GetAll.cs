using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                    return response.Match(Results.NotFound, Results.Ok);
                })
            .RequireAuthorization()
            .WithTags(EndpointTags.JsonData);
    }
}