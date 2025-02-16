namespace SimpleUrlShortener.Analytics.Domain.UseCases.GetAll;

public interface IGetAllStorage
{
    Task<IEnumerable<Url>> GetAll(string search = "", CancellationToken ct = default);
}