using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SimpleUrlShortener.Analytics.Presentation.Identity;

internal static class ApiKeyAuthenticationDefaults
{
    public const string AuthenticationScheme = "ApiKey";
    public const string ApiKeyHeaderName = "X-API-Key";
}

internal sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiKeyValidator apiKeyValidator)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!TryGetApiKey(out var apiKey, out var failureMessage))
        {
            return AuthenticateResult.Fail(failureMessage!);
        }

        try
        {
            var result = await apiKeyValidator.Validate(apiKey);
            if (!result)
            {
                failureMessage = "Invalid API Key";
                return AuthenticateResult.Fail(failureMessage);
            }

            var claims = new[] {
                new Claim(ApiKeyAuthenticationDefaults.ApiKeyHeaderName, apiKey)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            failureMessage = "An error occurred during authentication.";
            Logger.LogError(ex, failureMessage);

            return AuthenticateResult.Fail(failureMessage);
        }
    }

    private bool TryGetApiKey(out string apiKey, out string? failureMessage)
    {
        apiKey = string.Empty;

        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.ApiKeyHeaderName, out var headerValues))
        {
            failureMessage = $"Missing '{ApiKeyAuthenticationDefaults.ApiKeyHeaderName}' header.";
            return false;
        }

        if (headerValues.Count != 1)
        {
            failureMessage = $"Expecting only a single '{ApiKeyAuthenticationDefaults.ApiKeyHeaderName}' header.";
            return false;
        }

        apiKey = headerValues.FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            failureMessage = $"'{ApiKeyAuthenticationDefaults.ApiKeyHeaderName}' header value is null or empty.";
            return false;
        }

        failureMessage = null;
        return true;
    }
}