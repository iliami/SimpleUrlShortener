namespace SimpleUrlShortener.AnalyticsCollector.Domain.Shared;

public abstract record EventBusMessage;

public record UrlCreatedMessage(char InstancePrefix, string UrlCode, string OriginalUrl, DateTimeOffset CreatedAt) : EventBusMessage; 
public record UrlRedirectedMessage(char InstancePrefix, string UrlCode, string OriginalUrl, DateTimeOffset CreatedAt, string IpAddress) : EventBusMessage; 
public record UrlDeletedMessage(char InstancePrefix, string UrlCode) : EventBusMessage;
