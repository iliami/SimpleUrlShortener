namespace SimpleUrlShortener.UrlShortener.Domain.Core;

public enum DomainExceptionCode
{
    NotFound = 404,
    Default = 500
}

public class DomainException(
    string message,
    DomainExceptionCode code = DomainExceptionCode.Default)
    : Exception(message)
{
    public DomainExceptionCode Code { get; } = code;
}