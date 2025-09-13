using Papernote.SharedMicroservices.Cache;

namespace Papernote.Notes.Infrastructure.Services;

public class AuthTokenCacheKeyStrategy : IAdvancedCacheKeyStrategy
{
    public string ServicePrefix => "auth";
    public string Version => "v1";

    public string GetRevokedTokenKey(string jti)
        => GetVersionedKey("token", "revoked", jti);

    public string GetPatternKey(string operation, string wildcard = "*")
        => $"{ServicePrefix}:{Version}:{operation}:{wildcard}";

    public string GetVersionedKey(string operation, params string[] segments)
        => $"{ServicePrefix}:{Version}:{operation}:{string.Join(":", segments)}";
}