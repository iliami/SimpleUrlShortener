using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.Persistence;

public class SimpleUrlRepository(AppDbContext dbContext) : IUrlWriteRepository, IUrlReadRepository
{
    public async Task<bool> Save(UrlMapping urlMapping, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = urlMapping.Map();
            dbContext.UrlMappings.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<UrlMapping?> GetByCode(
        in UrlCode urlCode,
        CancellationToken cancellationToken = default)
    {
        var code = urlCode.Value;
        return dbContext.UrlMappings
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken)
            .Map();
    }
}