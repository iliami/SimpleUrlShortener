using Microsoft.Extensions.DependencyInjection;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Shared;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.EventConsumers.EventHandlers;

public interface IEventMessageHandlerProvider
{
    IEventMessageHandler? GetHandler<T>(T obj) where T : EventBusMessage;
}

public sealed class EventMessageHandlerProvider(IServiceProvider serviceProvider)
    : IEventMessageHandlerProvider, IAsyncDisposable
{
    private AsyncServiceScope ServiceScope { get; init; } = serviceProvider.CreateAsyncScope();

    public IEventMessageHandler? GetHandler<T>(T obj)
        where T : EventBusMessage
    {
        var objType = obj.GetType();
        var genericHandlerType = typeof(IEventMessageHandler<>).MakeGenericType(objType);
        return ServiceScope.ServiceProvider.GetService(genericHandlerType) as IEventMessageHandler;
    }

    public async ValueTask DisposeAsync()
    {
        await ServiceScope.DisposeAsync();
    }
}