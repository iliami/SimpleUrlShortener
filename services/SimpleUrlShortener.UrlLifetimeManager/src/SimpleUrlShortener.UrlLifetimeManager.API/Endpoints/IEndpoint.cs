namespace SimpleUrlShortener.UrlLifetimeManager.API.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}