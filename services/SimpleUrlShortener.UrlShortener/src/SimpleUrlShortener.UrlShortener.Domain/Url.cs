namespace SimpleUrlShortener.UrlShortener.Domain;

public class Url
{
    public const int OriginalMaxLength = 2048;
    public const int CodeMaxLength = 7;
    public string Original { get; private set; }
    public string Code { get; private set; }

    private Url(string original, string code)
    {
        Original = original;
        Code = code;
    }

    public static Url Create(string original, string code)
    {
        var url = new Url(original, code);
        return url;
    }
}