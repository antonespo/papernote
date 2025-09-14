using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Papernote.SharedMicroservices.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class InternalApiKeyAttribute : Attribute, IAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-Internal-ApiKey";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<InternalApiKeyAttribute>>();

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey))
        {
            logger.LogWarning("Internal API call attempted without API key from {RemoteIpAddress}", 
                context.HttpContext.Connection.RemoteIpAddress);
            
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Internal API access denied",
                detail = "Missing internal API key"
            });
            return;
        }

        var configuredApiKeys = configuration.GetSection("InternalApi:AllowedApiKeys").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
        
        if (configuredApiKeys.Count == 0)
        {
            logger.LogError("No internal API keys configured in InternalApi:AllowedApiKeys section");
            
            context.Result = new StatusCodeResult(500);
            return;
        }

        var isValidApiKey = configuredApiKeys.Values.Any(key => 
            !string.IsNullOrEmpty(key) && 
            string.Equals(key, providedApiKey, StringComparison.Ordinal));

        if (!isValidApiKey)
        {
            logger.LogWarning("Internal API call attempted with invalid API key from {RemoteIpAddress}. Key: {ApiKey}", 
                context.HttpContext.Connection.RemoteIpAddress,
                providedApiKey.ToString().Substring(0, Math.Min(8, providedApiKey.ToString().Length)) + "...");
            
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Internal API access denied",
                detail = "Invalid internal API key"
            });
            return;
        }

        var serviceName = configuredApiKeys.FirstOrDefault(kvp => 
            string.Equals(kvp.Value, providedApiKey, StringComparison.Ordinal)).Key ?? "Unknown";

        logger.LogDebug("Valid internal API call from service: {ServiceName}", serviceName);
        
        context.HttpContext.Items["InternalServiceName"] = serviceName;
    }
}