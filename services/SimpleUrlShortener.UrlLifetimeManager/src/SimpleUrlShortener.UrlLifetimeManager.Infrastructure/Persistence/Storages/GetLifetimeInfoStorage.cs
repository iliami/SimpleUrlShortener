using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application.UseCase;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Persistence.Storages;

public class GetLifetimeInfoStorage(
    AppDbContext dbContext)
    : IGetLifetimeInfoStorage
{
    public Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct)
        => dbContext.UrlMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code.Value, ct)
            .Map();
}