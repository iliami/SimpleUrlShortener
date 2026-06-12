using System.Net;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;
using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.API.Endpoints;

public class GetOriginalUrlEndpoint : IEndpoint
{
    private static IPAddress DefaultIpAddress = IPAddress.Parse("8.8.8.8");

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/{urlCode:required}", Handle);
    }

    private static async Task<IResult> Handle(
        string urlCode,
        [FromServices] IMediator mediator,
        [FromServices] ILogger<GetOriginalUrlEndpoint> logger,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress =
                httpContext.Connection.RemoteIpAddress ??
                DefaultIpAddress; // TODO: RemoteIpAddress can be the ip address of proxy (i.e. nginx)
            var request = new GetOriginalUrlRequest(
                new UrlCode(urlCode[1..]),
                new GetOriginalUrlRequest.RequestMetadata(ipAddress));
            var response = await mediator.Send(request, cancellationToken);
            return TypedResults.Redirect(response.Original.Value);
        }
        catch (DomainException ex) when (ex.Code == DomainExceptionCode.NotFound)
        {
            return TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError("Exception at GetOriginalUrlEndpoint {Exception}", ex);
            return TypedResults.InternalServerError();
        }
    }
}