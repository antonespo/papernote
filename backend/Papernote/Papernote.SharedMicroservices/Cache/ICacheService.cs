namespace Papernote.SharedMicroservices.Cache;

/// <summary>
/// Base interface for all cache operations across microservices
/// </summary>
public interface IBaseCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T item, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Extended interface for advanced Redis operations
/// </summary>
public interface IAdvancedCacheService : IBaseCacheService
{
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task IncrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<long> GetCounterAsync(string key, CancellationToken cancellationToken = default);
    Task SetIfNotExistsAsync<T>(string key, T item, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
}