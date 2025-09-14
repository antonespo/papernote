using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Papernote.SharedMicroservices.Security;

public class InternalApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InternalApiKeyMiddleware> _logger;
    private const string ApiKeyHeaderName = "X-Internal-ApiKey";

    public InternalApiKeyMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<InternalApiKeyMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        
        if (IsInternalApiPath(path))
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey))
            {
                _logger.LogWarning("Internal API call to {Path} attempted without API key from {RemoteIpAddress}", 
                    path, context.Connection.RemoteIpAddress);
                
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal API access denied",
                    detail = "Missing internal API key"
                });
                return;
            }

            var configuredApiKeys = _configuration.GetSection("InternalApi:AllowedApiKeys").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            
            if (configuredApiKeys.Count == 0)
            {
                _logger.LogError("No internal API keys configured in InternalApi:AllowedApiKeys section");
                
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal server error",
                    detail = "Internal API configuration error"
                });
                return;
            }

            var isValidApiKey = configuredApiKeys.Values.Any(key => 
                !string.IsNullOrEmpty(key) && 
                string.Equals(key, providedApiKey, StringComparison.Ordinal));

            if (!isValidApiKey)
            {
                _logger.LogWarning("Internal API call to {Path} attempted with invalid API key from {RemoteIpAddress}. Key: {ApiKey}", 
                    path, context.Connection.RemoteIpAddress,
                    providedApiKey.ToString().Substring(0, Math.Min(8, providedApiKey.ToString().Length)) + "...");
                
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal API access denied",
                    detail = "Invalid internal API key"
                });
                return;
            }

            var serviceName = configuredApiKeys.FirstOrDefault(kvp => 
                string.Equals(kvp.Value, providedApiKey, StringComparison.Ordinal)).Key ?? "Unknown";

            _logger.LogDebug("Valid internal API call to {Path} from service: {ServiceName}", path, serviceName);
            
            context.Items["InternalServiceName"] = serviceName;
        }

        await _next(context);
    }

    private static bool IsInternalApiPath(string path)
    {
        return path.StartsWith("/api/internal", StringComparison.OrdinalIgnoreCase);
    }
}