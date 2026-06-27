using Mediator;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.UrlShortener.Domain.Application.UseCases;
using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.API.Endpoints;

public record CreateShortUrlEndpointResponse(string OriginalUrl, string ShortenedUrl);

public class CreateShortUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/", Handle);
    }

    private static async Task<IResult> Handle(
        [FromQuery] string u,
        [FromServices] IMediator mediator,
        [FromServices] ILogger<CreateShortUrlEndpoint> logger,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (u is null or "")
        {
            return TypedResults.BadRequest();
        }

        try
        {
            var request = new CreateShortUrlRequest(new OriginalUrl(u));
            var response = await mediator.Send(request, cancellationToken);
            var host = httpContext.Request.Host.Value;
            if (host is null)
            {
                logger.LogCritical("Host is null at CreateShortUrlEndpoint");
                return Results.InternalServerError();
            }

            var scheme = httpContext.Request.Scheme;
            var shortUrl = $"{scheme}://{host}/api/{response.CodePrefix}{response.Code.Value}";

            return TypedResults.Ok(new CreateShortUrlEndpointResponse(
                request.Original.Value,
                shortUrl
            ));
        }
        catch (Exception ex)
        {
            logger.LogError("Exception at CreateShortUrlEndpoint {Exception}", ex);
            return TypedResults.InternalServerError();
        }
    }
}