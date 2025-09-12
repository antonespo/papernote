namespace Papernote.Auth.Core.Application.Interfaces;

public interface IAuthCacheKeyStrategy
{
    string GetUserResolutionKey(string username);
    string GetUserIdResolutionKey(Guid userId);
    string GetUserResolutionPatternKey();
    string GetRateLimitKey(string username);
    string GetRevokedTokenKey(string jti);
}