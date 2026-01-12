namespace SimpleUrlShortener.UrlShortener.Domain;

public static class UrlCodeSettings
{
    public const int CodeLength = Url.CodeMaxLength;
    public const string Alphabet = "qwertyuiopasdfghjklzxcvbnmQWER0123456789";
    public const int MaxAttemptsToResolveCollision = 100;
}