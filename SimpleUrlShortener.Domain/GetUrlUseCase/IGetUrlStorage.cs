namespace SimpleUrlShortener.Domain.GetUrlUseCase;

public interface IGetUrlStorage
{
    public Task<string?> GetUrl(string urlCode, CancellationToken ct = default);
}