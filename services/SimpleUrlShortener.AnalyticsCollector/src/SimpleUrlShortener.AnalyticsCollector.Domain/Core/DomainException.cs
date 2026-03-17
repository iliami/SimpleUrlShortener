using System.Collections;

namespace SimpleUrlShortener.AnalyticsCollector.Domain.Core;

public class DomainException(string message) : Exception(message);

public class NotFoundException : DomainException
{
    public NotFoundException(Type entityType) : base($"Entity of type {entityType} not found")
    {
        EntityType = entityType;
        Data = new Dictionary<string, string>();
    }

    public NotFoundException(Type entityType, Dictionary<string, string> metadata) : base($"Entity of type {entityType} not found")
    {
        EntityType = entityType;
        Data = metadata;
    }

    public Type EntityType { get; }
    public override IDictionary Data { get; }
}