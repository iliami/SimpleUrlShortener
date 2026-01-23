namespace SimpleUrlShortener.UrlShortener.Domain.Core;

public class UrlShortenerSettings
{
    public const int TotalCodeLength = 7;
    public const string CodeAlphabet = "0123456789abcdefghigklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public int MaxAttemptsToResolveCollision { get => Math.Clamp(field, 1, 10000000); set; } = 100;
    public char InstancePrefix { get; set; }
}

public record struct ShardId
{
    private static HashSet<char> ValueRange { get; } = new(UrlShortenerSettings.CodeAlphabet);
    public char Value { get; private init; }

    public ShardId(char value)
    {
        if (!ValueRange.Contains(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        Value = value;
    }
}