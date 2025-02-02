namespace SimpleUrlShortener.Infrastructure;

public interface IMomentProvider
{
    DateTimeOffset Current { get; }
}

public class MomentProvider : IMomentProvider
{
    public DateTimeOffset Current => DateTimeOffset.UtcNow;
}