namespace SimpleUrlShortener.Domain;

public static class UrlCodeSettings
{
    public const int CodeLength = 7;
    public const string Alphabet = "qwertyuiopasdfghjklzxcvbnmQWER0123456789";
    public const int MaxAttemptsToResolveCollision = 100;
}