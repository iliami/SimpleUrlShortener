using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SimpleUrlShortener.UrlShortener.Presentation.Filters;

[AttributeUsage(AttributeTargets.Method |  AttributeTargets.Class, AllowMultiple = true)]
public class ApiAuthorizationFilterAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var path = context.HttpContext.Request.Path.Value!;
        if (!path.Contains("/api/"))
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyValues) ||
            apiKeyValues.Count != 1)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Unauthorized",
                message = "API Key header is missing"
            });
            return;
        }
        var apiKey = apiKeyValues[0];

        var configuration = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>();
        var expectedKey = configuration["ApiKey"];

        if (apiKey != expectedKey)
        {
            context.Result = new ObjectResult(new
            {
                error = "Forbidden",
                message = "Invalid API Key"
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };

            return;
        }

        await next();
    }
}
