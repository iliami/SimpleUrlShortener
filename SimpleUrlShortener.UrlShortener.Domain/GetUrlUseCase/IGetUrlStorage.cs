namespace SimpleUrlShortener.UrlShortener.Domain.GetUrlUseCase;

public interface IGetUrlStorage
{
    public Task<Url?> GetUrl(string urlCode, CancellationToken ct = default);
}