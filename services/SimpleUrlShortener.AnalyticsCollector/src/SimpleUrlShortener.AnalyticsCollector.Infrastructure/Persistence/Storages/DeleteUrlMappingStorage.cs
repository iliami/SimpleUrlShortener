using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application.UseCase;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Persistence.Storages;

public class DeleteUrlMappingStorage(
    AppDbContext dbContext) : IDeleteUrlMappingStorage
{
    public Task<UrlMapping?> TryGet(UrlCode code, CancellationToken ct)
        => dbContext.UrlMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code.Value, ct)
            .Map();

    public async Task<bool> Save(UrlMappingDeletion urlMappingDeletion, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = urlMappingDeletion.Map();

            dbContext.UrlMappingDeletions.Attach(entity);
            var addedUrlMappingDeletion = await dbContext.SaveChangesAsync(cancellationToken);

            await dbContext.UrlMappingRedirections
                .Where(x => x.UrlMappingId == urlMappingDeletion.Code.Value)
                .ExecuteUpdateAsync(
                    x => x.SetProperty(
                        d => d.UrlMappingDeletionId,
                        entity.Id),
                    cancellationToken);

            var deletedUrlMapping = await dbContext.UrlMappings
                .Where(x => x.Code == urlMappingDeletion.Code.Value)
                .ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return addedUrlMappingDeletion == 1 && deletedUrlMapping == 1;
        }
        catch
        {
            return false;
        }
        finally
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}