using Microsoft.Extensions.Logging;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Application;
using SimpleUrlShortener.UrlLifetimeManager.Domain.Core;

namespace SimpleUrlShortener.UrlLifetimeManager.Infrastructure.Clients;

public class UrlShortenerHttpClient(
    HttpClient httpClient,
    ILogger<UrlShortenerHttpClient> logger)
    : IUrlShortenerClient
{
    public async Task Delete(UrlCode urlCode, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.DeleteAsync(
                $"api/{urlCode.Value}", 
                cancellationToken);

            response.EnsureSuccessStatusCode();

            logger.LogInformation("UrlShortener delete success: {UrlCode}", urlCode);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "UrlShortener delete failed: {UrlCode}", urlCode);
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("UrlShortener delete cancelled: {UrlCode}", urlCode);
            throw;
        }
    }
}