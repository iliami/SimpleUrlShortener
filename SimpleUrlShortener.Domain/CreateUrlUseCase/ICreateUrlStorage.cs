namespace SimpleUrlShortener.Domain.CreateUrlUseCase;

public interface ICreateUrlStorage
{
    public Task<Url> CreateUrl(UrlDto urlDto, CancellationToken ct = default);
}