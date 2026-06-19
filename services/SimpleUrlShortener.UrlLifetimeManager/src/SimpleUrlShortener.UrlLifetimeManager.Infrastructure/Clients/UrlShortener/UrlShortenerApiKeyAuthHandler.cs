using Microsoft.Extensions.Options;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Clients.UrlShortener;

public class UrlShortenerApiKeyAuthHandler(
    IOptionsMonitor<UrlShortenerOptions> optionsMonitor)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var apiKey = optionsMonitor.CurrentValue.ApiKey;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Remove("X-API-KEY");
            request.Headers.Add("X-API-KEY", apiKey);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}