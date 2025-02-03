using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.Domain.Events;
using SimpleUrlShortener.Infrastructure.Entities;
using Url = SimpleUrlShortener.Infrastructure.Entities.Url;

namespace SimpleUrlShortener.Infrastructure.Consumers;

public class UrlCreatedEventConsumer(
    NoTrackingDbContext dbContext,
    IStringCacheStorage stringCacheStorage,
    IGuidFactory guidFactory,
    ILogger<UrlCreatedEventConsumer> logger) : IConsumer<UrlCreatedEvent>
{
    public async Task Consume(ConsumeContext<UrlCreatedEvent> context)
    {
        var url = await dbContext.Urls
            .FirstOrDefaultAsync(u => u.Original == context.Message.Original);

        if (url is not null)
        {
            if (url.Code != context.Message.Code)
            {
                logger.LogError("Url {Url} code {Code} mismatch with stored code {StoredCode}", 
                    context.Message.Original, context.Message.Code, url.Code);
                throw new Exception($"Url {context.Message.Original} code {context.Message.Code} mismatch with stored code {url.Code}");
            }

            var creationMoment = new UrlCreationMoment
            {
                Id = guidFactory.Create(),
                CreatedAt = context.Message.CreatedAt,
                Url = url,
            };
            dbContext.Entry(url).State = EntityState.Unchanged;
            await dbContext.UrlCreationMoments.AddAsync(creationMoment);
        }
        else
        {
            url = new Url
            {
                Id = guidFactory.Create(),
                Original = context.Message.Original,
                Code = context.Message.Code,
            };
            var creationMoment = new UrlCreationMoment
            {
                Id = guidFactory.Create(),
                CreatedAt = context.Message.CreatedAt,
                Url = url,
            };
            url.CreationMoments.Add(creationMoment);
            await dbContext.Urls.AddAsync(url);
        }
        
        await dbContext.SaveChangesAsync();

        await stringCacheStorage.Set(url.Original, url.Code, TimeSpan.FromHours(12));
    }
}