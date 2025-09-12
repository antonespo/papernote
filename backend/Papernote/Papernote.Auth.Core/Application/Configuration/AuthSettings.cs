namespace Papernote.Auth.Core.Application.Configuration;

public class AuthSettings
{
    public const string SectionName = "Auth";
    
    public JwtSettings JwtSettings { get; set; } = new();
    public PasswordSettings PasswordSettings { get; set; } = new();
}

public class JwtSettings
{
    public string Issuer { get; set; } = "papernote-auth";
    public string Audience { get; set; } = "papernote-api";
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(7);
    public string SigningKey { get; set; } = string.Empty;
}

public class PasswordSettings
{
    public int RequiredLength { get; set; } = 8;
    public bool RequireDigit { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public int MaxFailedAttempts { get; set; } = 5;
}