namespace Papernote.SharedMicroservices.Configuration;

public class InternalApiSettings
{
    public const string SectionName = "InternalApi";
    
    public Dictionary<string, string> AllowedApiKeys { get; set; } = new();
}

public static class InternalApiExtensions
{
    public static string? GetApiKeyForService(this InternalApiSettings settings, string serviceName)
    {
        return settings.AllowedApiKeys.TryGetValue(serviceName, out var apiKey) ? apiKey : null;
    }
    
    public static bool IsValidApiKey(this InternalApiSettings settings, string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return false;
            
        return settings.AllowedApiKeys.Values.Any(key => 
            !string.IsNullOrEmpty(key) && 
            string.Equals(key, apiKey, StringComparison.Ordinal));
    }
    
    public static string? GetServiceNameForApiKey(this InternalApiSettings settings, string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return null;
            
        return settings.AllowedApiKeys.FirstOrDefault(kvp => 
            string.Equals(kvp.Value, apiKey, StringComparison.Ordinal)).Key;
    }
}