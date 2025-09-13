using Papernote.Auth.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Cache;

namespace Papernote.Auth.Infrastructure.Cache;

public class AuthCacheKeyStrategy : IAuthCacheKeyStrategy, IAdvancedCacheKeyStrategy
{
    public string ServicePrefix => "auth";
    public string Version => "v1";

    public string GetUserResolutionKey(string username)
        => GetVersionedKey("users", "username", username.ToLowerInvariant());

    public string GetUserIdResolutionKey(Guid userId)
        => GetVersionedKey("users", "userid", userId.ToString());

    public string GetUserResolutionPatternKey()
        => GetPatternKey("users", "*");

    public string GetRateLimitKey(string username)
        => GetVersionedKey("ratelimit", "login", username.ToLowerInvariant());

    public string GetRevokedTokenKey(string jti)
        => GetVersionedKey("token", "revoked", jti);

    public string GetPatternKey(string operation, string wildcard = "*")
        => $"{ServicePrefix}:{Version}:{operation}:{wildcard}";

    public string GetVersionedKey(string operation, params string[] segments)
        => $"{ServicePrefix}:{Version}:{operation}:{string.Join(":", segments)}";
}