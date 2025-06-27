using System.Net;
using AAI.Core;

namespace AAI.Rest.Services.Authentication;

public class ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(AuthApiOptions.ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Api key is missing.");
            return;
        }

        var apiKey = configuration
            .GetSection(AuthApiOptions.AuthOptionsSectionName)
            .Get<AuthApiOptions>()!
            .ApiKey;
        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid Api Key.");
            return;
        }

        await next(context);
    }
}