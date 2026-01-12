using Microsoft.EntityFrameworkCore;
using SimpleUrlShortener.UrlShortener.Domain;
using SimpleUrlShortener.UrlShortener.Domain.CreateUrlUseCase;
using SimpleUrlShortener.UrlShortener.Infrastructure.Entities;
using Url = SimpleUrlShortener.UrlShortener.Domain.Url;

namespace SimpleUrlShortener.UrlShortener.Infrastructure;

public class CreateUrlStorage(
    ICacheStorage cacheStorage,
    NoTrackingDbContext dbContext,
    IGuidFactory guidFactory,
    IMomentProvider momentProvider) : ICreateUrlStorage
{
    public async Task<Url> CreateUrl(string original, string code, CancellationToken ct = default)
    {
        await dbContext.Database.BeginTransactionAsync(ct);

        var url = await dbContext.Urls
            .FirstOrDefaultAsync(u => u.Original == original, ct);

        if (url is not null)
        {
            if (url.Code != code)
            {
                // logger.LogError("Url {Url} code {Code} mismatch with stored code {StoredCode}", 
                //     context.Message.Original, context.Message.Code, url.Code);
                throw new Exception($"Url {original} code {code} mismatch with stored code {url.Code}");
            }

            var creationMoment = new UrlCreationMoment
            {
                Id = guidFactory.Create(),
                CreatedAt = momentProvider.Current,
                Url = url,
            };
            dbContext.Entry(url).State = EntityState.Unchanged;
            await dbContext.UrlCreationMoments.AddAsync(creationMoment, ct);
        }
        else
        {
            url = new Entities.Url
            {
                Id = guidFactory.Create(),
                Original = original, 
                Code = code
            };
            var creationMoment = new UrlCreationMoment
            {
                Id = guidFactory.Create(),
                CreatedAt = momentProvider.Current,
                Url = url,
            };
            url.CreationMoments.Add(creationMoment);
            await dbContext.Urls.AddAsync(url, ct);
        }
        
        await dbContext.SaveChangesAsync(ct);
        await dbContext.Database.CommitTransactionAsync(ct);

        await cacheStorage.Set(code, url, TimeSpan.FromHours(12), ct);

        return Url.Create(original, code);
    }
}