namespace SimpleUrlShortener.UrlShortener.API.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}