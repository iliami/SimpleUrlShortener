using SimpleUrlShortener.UrlShortener.Domain.Core;

namespace SimpleUrlShortener.UrlShortener.Domain.Application;

public interface IShardProvider
{
    IUrlWriteRepository GetWriteRepository(ShardId shardId);
    IUrlReadRepository GetReadRepository(ShardId shardId);
}