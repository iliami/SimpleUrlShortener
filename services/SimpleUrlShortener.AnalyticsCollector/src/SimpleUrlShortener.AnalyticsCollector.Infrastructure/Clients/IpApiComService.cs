using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SimpleUrlShortener.AnalyticsCollector.Domain.Application;
using SimpleUrlShortener.AnalyticsCollector.Domain.Core;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Clients;

public sealed class IpApiComService(
    HttpClient httpClient,
    ILogger<IpApiComService> logger)
    : IGeoIpService
{
    private const int BatchSize = 100;

    private const string BatchEndpoint =
        "batch?fields=status,message,lat,lon,query";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<Dictionary<Ip, Coordinates>> GetCoordinates(
        IEnumerable<Ip> ips,
        CancellationToken cancellationToken)
    {
        var batches = ips
            .Where(static ip => ip.Kind == IpKind.Ipv4Public)
            .Select(static ip => ip.Value)
            .Chunk(BatchSize)
            .ToArray();

        if (batches.Length == 0)
        {
            return new Dictionary<Ip, Coordinates>();
        }

        var batchTasks = batches
            .Select(batch => FetchBatchAsync(batch, cancellationToken));

        var batchResults =
            await Task.WhenAll(batchTasks).ConfigureAwait(false);

        return batchResults
            .SelectMany(static batch => batch)
            .GroupBy(static pair => pair.Key)
            .ToDictionary(
                static group => group.Key,
                static group => group.First().Value);
    }

    private async Task<IReadOnlyList<KeyValuePair<Ip, Coordinates>>> FetchBatchAsync(
        string[] ipBatch,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient
                .PostAsJsonAsync(
                    BatchEndpoint,
                    ipBatch,
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                logger.LogError(
                    "IP API request failed. Status: {StatusCode}, Body: {Body}",
                    response.StatusCode,
                    errorBody);

                response.EnsureSuccessStatusCode();
            }

            var items = await response.Content
                .ReadFromJsonAsync<IpApiComResponse?[]>(
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            if (items is null || items.Length == 0)
            {
                return [];
            }

            var successful =
                new List<KeyValuePair<Ip, Coordinates>>(items.Length);

            foreach (var item in items)
            {
                if (item is null)
                {
                    continue;
                }

                if (!string.Equals(item.Status, "success", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogError(
                        "Failed for IP {Ip} with message {Message}",
                        item.Ip,
                        item.Message);

                    continue;
                }

                if (item.Ip is null || item.Latitude is null || item.Longitude is null)
                {
                    logger.LogWarning(
                        "Success response for IP {Ip} does not contain coordinates.",
                        item.Ip);

                    continue;
                }

                var ip = new Ip(item.Ip);
                var coordinates = new Coordinates(
                    item.Latitude.Value,
                    item.Longitude.Value);

                successful.Add(new KeyValuePair<Ip, Coordinates>(ip, coordinates));
            }

            return successful;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Batch request failed for {IpCount} IPs.",
                ipBatch.Length);

            throw;
        }
    }

    private sealed class IpApiComResponse
    {
        [JsonPropertyName("status")] public string? Status { get; init; }

        [JsonPropertyName("message")] public string? Message { get; init; }

        [JsonPropertyName("lat")] public double? Latitude { get; init; }

        [JsonPropertyName("lon")] public double? Longitude { get; init; }

        [JsonPropertyName("query")] public string? Ip { get; init; }
    }
}
//     var (latitude, longitude) = ip.Kind switch
//     {
//         IpKind.Ipv4Public => (55.751244, 37.618423), // Moscow
//         IpKind.Ipv4Private => (40.711967, -74.006076), // New York
//         IpKind.Ipv6 => (39.916668, 116.383331), // Beijing
//         _ => throw new ArgumentOutOfRangeException(nameof(ip), ip, "Invalid ip kind")
//     };