namespace SimpleUrlShortener.Domain;

public static class UrlCodeSettings
{
    public const int CodeLength = 7;
    public const string Alphabet = "qwertyuiopasdfghjklzxcvbnm.,?0123456789";
    public const int MaxAttemptsToResolveCollision = 100;
}