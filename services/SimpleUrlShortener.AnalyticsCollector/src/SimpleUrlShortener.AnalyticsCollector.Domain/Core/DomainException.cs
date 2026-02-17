namespace SimpleUrlShortener.AnalyticsCollector.Domain.Core;

public class DomainException(string message) : Exception(message);

public class NotFoundException(Type entityType) : DomainException($"Entity of type {entityType} not found")
{
    public Type EntityType { get; } = entityType;
}