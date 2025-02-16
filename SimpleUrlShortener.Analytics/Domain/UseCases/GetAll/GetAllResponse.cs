namespace SimpleUrlShortener.Analytics.Domain.UseCases.GetAll;

public record GetAllResponse(IEnumerable<Url> Urls);