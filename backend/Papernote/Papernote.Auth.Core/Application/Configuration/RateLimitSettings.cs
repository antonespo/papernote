namespace Papernote.Auth.Core.Application.Configuration;

public class RateLimitSettings
{
    public const string SectionName = "RateLimit";

    public int WindowMinutes { get; set; } = 5;
    public int MaxAttempts { get; set; } = 5;
}