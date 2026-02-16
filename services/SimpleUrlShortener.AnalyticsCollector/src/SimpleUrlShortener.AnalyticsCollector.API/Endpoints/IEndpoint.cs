namespace SimpleUrlShortener.AnalyticsCollector.API.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}