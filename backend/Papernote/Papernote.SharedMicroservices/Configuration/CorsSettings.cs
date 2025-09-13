namespace Papernote.SharedMicroservices.Configuration;

public class CorsSettings
{
    public const string SectionName = "Cors";
    
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; } = false;
    public string PolicyName { get; set; } = "DefaultCorsPolicy";
}