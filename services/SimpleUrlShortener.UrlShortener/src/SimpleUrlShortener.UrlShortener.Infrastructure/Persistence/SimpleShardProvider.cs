using Microsoft.Extensions.DependencyInjection;
using SimpleUrlShortener.UrlShortener.Domain.Application;
using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Infrastructure.Persistence;

public class SimpleShardProvider(IServiceProvider serviceProvider) : IShardProvider
{
    private Dictionary<ShardId, IUrlWriteRepository> WriteRepos { get; } = new();
    private Dictionary<ShardId, IUrlReadRepository> ReadRepos { get; } = new();

    public IUrlWriteRepository GetWriteRepository(ShardId shardId)
    {
        if (WriteRepos.TryGetValue(shardId, out var repository))
        {
            return repository;
        }
        // var writeRepository =  serviceProvider.GetRequiredKeyedService<IUrlWriteRepository>(shardId.Value);
        var writeRepository =  serviceProvider.GetRequiredService<IUrlWriteRepository>();
        WriteRepos.Add(shardId, writeRepository);
        return writeRepository;
    }

    public IUrlReadRepository GetReadRepository(ShardId shardId)
    {
        if (ReadRepos.TryGetValue(shardId, out var repository))
        {
            return repository;
        }
        // var readRepository = serviceProvider.GetRequiredKeyedService<IUrlReadRepository>(shardId.Value);
        var readRepository = serviceProvider.GetRequiredService<IUrlReadRepository>();
        ReadRepos.Add(shardId, readRepository);
        return readRepository;
    }
}