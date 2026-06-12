namespace SimpleUrlShortener.AnalyticsCollector.Domain.Core;

public class DomainException(string message) : Exception(message);

public class NotFoundException<T> : DomainException
{
    public NotFoundException() : base($"Entity of type {typeof(T)} not found")
    {
    }

    public NotFoundException(string info) : base($"Entity of type {typeof(T)} not found: {info}")
    {
    }
}