namespace SimpleUrlShortener.Analytics.Domain;

public class Url
{
    public string Original { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public IEnumerable<DateTimeOffset> CreationMoments { get; private set; } = [];
    public IEnumerable<DateTimeOffset> ClickMoments { get; private set; } = [];

    private Url() {}

    public static Url Create(
        string original,
        string code,
        IEnumerable<DateTimeOffset> creationMoments,
        IEnumerable<DateTimeOffset> clickMoments) => new()
    {
        Original = original,
        Code = code,
        CreationMoments = creationMoments,
        ClickMoments = clickMoments
    };
}