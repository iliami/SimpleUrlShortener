namespace SimpleUrlShortener.Domain;

public class Url
{
    public Guid Id { get; set; }
    public string Original { get; private set; }
    public string Normalized { get; private set; }
    public string Code { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Url(Guid id, string original, string normalized, string code, DateTimeOffset createdAt)
    {
        Id = id;
        Original = original;
        Normalized = normalized;
        Code = code;
        CreatedAt = createdAt;
    }

    public static Url Create(Guid id, string original, string normalized, string code, DateTimeOffset createdAt)
    {
        var url = new Url(id, original, normalized, code, createdAt);
        return url;
    }
}