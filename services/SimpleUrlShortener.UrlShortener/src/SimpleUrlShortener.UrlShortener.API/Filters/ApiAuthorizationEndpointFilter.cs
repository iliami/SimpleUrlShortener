using Microsoft.Extensions.Primitives;

public class ApiAuthorizationEndpointFilter : IEndpointFilter
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        if (!httpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out StringValues apiKeyValues) ||
            apiKeyValues.Count != 1)
        {
            return Results.Json(new
            {
                error = "Unauthorized",
                message = "API Key header is missing"
            }, statusCode: StatusCodes.Status401Unauthorized);
        }

        var apiKey = apiKeyValues[0];

        var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedKey = configuration["ApiKey"];

        if (apiKey != expectedKey)
        {
            return Results.Json(new
            {
                error = "Forbidden",
                message = "Invalid API Key"
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}