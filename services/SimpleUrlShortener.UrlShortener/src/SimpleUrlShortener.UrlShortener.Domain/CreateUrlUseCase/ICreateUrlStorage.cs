namespace SimpleUrlShortener.UrlShortener.Domain.CreateUrlUseCase;

public interface ICreateUrlStorage
{
    public Task<Url> CreateUrl(string original, string code, CancellationToken ct = default);
}