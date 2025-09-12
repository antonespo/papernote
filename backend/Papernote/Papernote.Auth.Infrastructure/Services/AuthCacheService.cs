using Microsoft.Extensions.Logging;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Cache;

namespace Papernote.Auth.Infrastructure.Services;

public class AuthCacheService
{
    private readonly IAdvancedCacheService _cacheService;
    private readonly IAuthCacheKeyStrategy _keyStrategy;
    private readonly ILogger<AuthCacheService> _logger;

    private const int USER_RESOLUTION_CACHE_MINUTES = 10;

    public AuthCacheService(
        IAdvancedCacheService cacheService,
        IAuthCacheKeyStrategy keyStrategy,
        ILogger<AuthCacheService> logger)
    {
        _cacheService = cacheService;
        _keyStrategy = keyStrategy;
        _logger = logger;
    }

    public async Task CacheUserResolutionAsync<T>(string key, T data, CancellationToken cancellationToken = default) where T : class
    {
        await _cacheService.SetAsync(key, data, TimeSpan.FromMinutes(USER_RESOLUTION_CACHE_MINUTES), cancellationToken);
    }

    public async Task<T?> GetUserResolutionAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        return await _cacheService.GetAsync<T>(key, cancellationToken);
    }

    public async Task InvalidateUserResolutionAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cacheService.RemoveAsync(key, cancellationToken);
    }

    public async Task InvalidateAllUserResolutionAsync(CancellationToken cancellationToken = default)
    {
        var pattern = _keyStrategy.GetUserResolutionPatternKey();
        await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
    }
}