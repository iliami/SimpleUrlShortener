namespace SimpleUrlShortener.Analytics.Domain;

public interface IConverter<in T>
{
    string Convert(T value);
}