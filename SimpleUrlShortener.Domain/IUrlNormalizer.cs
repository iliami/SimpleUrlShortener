namespace SimpleUrlShortener.Domain;

public interface IUrlNormalizer
{
    Task<string> NormalizeUrl(string url, CancellationToken ct);
}

public class UrlNormalizer : IUrlNormalizer
{
    private static readonly string[] Protocols = ["http://", "https://"]; 
    public Task<string> NormalizeUrl(string url, CancellationToken ct)
    {
        // Добавляем протокол, если его нет
        var isAnyProtocol = Protocols.Aggregate(false,
            (current, protocol) => current | url.StartsWith(protocol));
        if (!isAnyProtocol)
        {
            url = "https://" + url;
        }

        // Создаем объект Uri
        Uri uri = new(url);

        // Удаляем 'www.' из хоста
        var host = uri.Host;
        if (host.StartsWith("www.") && host.Count(s => s == '.') > 1)
        {
            host = host[4..];
        }

        // Убираем слеш в конце пути, если он есть
        var path = uri.AbsolutePath.TrimEnd('/');

        // Собираем URL обратно, сохраняя исходный протокол
        var uriBuilder = new UriBuilder
        {
            Scheme = uri.Scheme,  // Сохраняем исходный протокол
            Host = host,
            Path = uri.AbsolutePath,
            Query = uri.Query,
            Fragment = uri.Fragment
        };

        return Task.FromResult(uriBuilder.Uri.ToString());
    }
}