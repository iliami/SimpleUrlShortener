using System.Net;
using System.Net.Http.Headers;

namespace SimpleUrlShortener.AnalyticsCollector.Infrastructure.Clients;

public sealed class IpApiRateLimitGuardHandler : DelegatingHandler
{
    private static readonly SemaphoreSlim Sync = new(1, 1);
    private static DateTimeOffset _blockedUntil = DateTimeOffset.MinValue;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        int ttlSeconds = 0;

        await Sync.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_blockedUntil > now)
            {
                ttlSeconds = (int)Math.Ceiling((_blockedUntil - now).TotalSeconds);
            }
        }
        finally
        {
            Sync.Release();
        }

        // Если X-Rl был 0, то не делаем реальный запрос, а возвращаем синтетический 429
        if (ttlSeconds > 0)
        {
            var syntheticResponse = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
            syntheticResponse.Headers.Add("X-Ttl", $"{ttlSeconds}");
            syntheticResponse.Headers.Add("Retry-After", $"{ttlSeconds}");
            return syntheticResponse;
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var is429 = response.StatusCode == HttpStatusCode.TooManyRequests;
        var xRlIsZero = TryGetInt(response.Headers, "X-Rl", out int remaining) && remaining == 0;

        if (is429 || xRlIsZero)
        {
            // Берем X-Ttl, либо ставим 0 (не блокируем, если сервер не сказал время), либо 60 по умолчанию для 429
            var newTtl = 0;
            if (TryGetInt(response.Headers, "X-Ttl", out var xTtl))
            {
                newTtl = xTtl;
            }
            else if (is429)
            {
                newTtl = 60;
            }

            if (newTtl <= 0)
            {
                return response;
            }

            var candidate = DateTimeOffset.UtcNow.AddSeconds(newTtl);

            await Sync.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (candidate > _blockedUntil)
                {
                    _blockedUntil = candidate;
                }
            }
            finally
            {
                Sync.Release();
            }
        }

        return response;
    }

    private static bool TryGetInt(HttpHeaders headers, string name, out int value)
    {
        if (headers.TryGetValues(name, out var values) &&
            int.TryParse(values.FirstOrDefault(), out value) &&
            value >= 0)
        {
            return true;
        }

        value = 0;
        return false;
    }
}