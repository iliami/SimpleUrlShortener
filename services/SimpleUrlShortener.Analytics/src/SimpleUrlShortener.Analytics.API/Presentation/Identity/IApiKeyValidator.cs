using Microsoft.Extensions.Options;

namespace SimpleUrlShortener.Analytics.Presentation.Identity;

public interface IApiKeyValidator
{
    Task<bool> Validate(string apiKey);
}

public class ApiKeyValidator(IOptionsSnapshot<ApiSettings> optionsSnapshot) : IApiKeyValidator
{
    private readonly ApiSettings _apiSettings = optionsSnapshot.Value
                                                ?? throw new ArgumentNullException(nameof(ApiSettings));

    public Task<bool> Validate(string apiKey)
        => Task.FromResult(apiKey == _apiSettings.AccessKey);
}