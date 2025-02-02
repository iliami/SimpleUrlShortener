using MassTransit;
using SimpleUrlShortener.Domain;

namespace SimpleUrlShortener.Infrastructure;

public class UrlCreatedEventConsumer(NoTrackingDbContext dbContext, IStringCacheStorage stringCacheStorage) : IConsumer<UrlCreatedEvent>
{
    public async Task Consume(ConsumeContext<UrlCreatedEvent> context)
    {
        var url = new Url
        {
            Id = context.Message.Id,
            Original = context.Message.Original,
            Code = context.Message.Code,
            CreatedAt = context.Message.CreatedAt
        };

        await dbContext.Urls.AddAsync(url);
        await dbContext.SaveChangesAsync();

        await stringCacheStorage.Set(url.Original, url.Code, TimeSpan.FromHours(12));
    }
}