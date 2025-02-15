using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.UrlShortener.Domain.Events;
using SimpleUrlShortener.UrlShortener.Infrastructure.Entities;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.Consumers;

public class UrlClickedEventConsumer(
    NoTrackingDbContext dbContext,
    IGuidFactory guidFactory,
    ILogger<UrlClickedEventConsumer> logger) : IConsumer<UrlClickedEvent>
{
    public async Task Consume(ConsumeContext<UrlClickedEvent> context)
    {
        var url = await dbContext.Urls.FirstOrDefaultAsync(u => u.Code == context.Message.Code);
        if (url is null)
        {
            logger.LogError("Url {Original} with the code {Code} not found", 
                context.Message.Original, context.Message.Code);
            throw new Exception($"Url {context.Message.Original} with the code {context.Message.Code} not found");
        }
        if (url.Original != context.Message.Original)
        {
            logger.LogError("Code {Code} of the original url {Original} mismatch with the stored original url {StoredOriginal}", 
                context.Message.Code, context.Message.Original, url.Original);
            throw new Exception($"Code {context.Message.Code} of the original url {context.Message.Original} mismatch with the stored original url {url.Original}");
        }

        var click = new UrlClick
        {
            Id = guidFactory.Create(),
            ClickedAt = context.Message.ClickedAt,
            Url = url
        };

        dbContext.Entry(url).State = EntityState.Unchanged;
        await dbContext.UrlClicks.AddAsync(click);
        await dbContext.SaveChangesAsync();
    }
}