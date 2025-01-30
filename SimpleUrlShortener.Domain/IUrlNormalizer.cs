namespace SimpleUrlShortener.Domain;

public interface IUrlNormalizer
{
    Task<string> NormalizeUrl(string url, CancellationToken ct);
}

public class UrlNormalizer : IUrlNormalizer
{
    private static readonly string[] Protocols = ["http://", "https://", "ftp://", "ftps://", "file://"]; 
    public Task<string> NormalizeUrl(string url, CancellationToken ct)
    {
        var isAnyProtocol = Protocols.Aggregate(false,
            (current, protocol) => current | url.StartsWith(protocol, StringComparison.InvariantCultureIgnoreCase));
        if (!isAnyProtocol)
        {
            url = "https://" + url;
        }

        Uri uri = new(url);

        var host = uri.Host;
        if (host.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase) && host.Count(s => s == '.') > 1)
        {
            host = host[4..];
        }

        var path = uri.AbsolutePath.TrimEnd('/');

        var uriBuilder = new UriBuilder
        {
            Scheme = uri.Scheme,
            Host = host,
            Path = uri.AbsolutePath,
            Query = uri.Query,
            Fragment = uri.Fragment
        };

        return Task.FromResult(uriBuilder.Uri.ToString());
    }
}