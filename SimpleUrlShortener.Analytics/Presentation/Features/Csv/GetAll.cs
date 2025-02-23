using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleUrlShortener.Analytics.Domain;
using SimpleUrlShortener.Analytics.Domain.UseCases.GetAll;
using SimpleUrlShortener.Analytics.Presentation.Identity;

namespace SimpleUrlShortener.Analytics.Presentation.Features.Csv;

public class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("csv", [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
                async (
                    [AsParameters] GetAllRequest request,
                    [FromServices] IMediator mediator,
                    [FromKeyedServices("CsvConverter")] IConverter<IEnumerable<Url>> converter,
                CancellationToken ct) =>
                {
                    var response = await mediator.Send(request, ct);

                    return response.Match(Results.NotFound, result =>
                    {
                        var csvContent = converter.Convert(result.Urls);

                        var bytes = Encoding.UTF8.GetBytes(csvContent);

                        return Results.File(bytes, "text/csv", "urls_analytics.csv");
                    });
                })
            .RequireAuthorization()
            .WithTags(EndpointTags.CsvData);
    }
}