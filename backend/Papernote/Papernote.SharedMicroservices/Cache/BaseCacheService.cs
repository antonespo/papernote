using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Papernote.SharedMicroservices.Cache;

/// <summary>
/// Base Redis cache service implementation for all microservices
/// </summary>
public class BaseCacheService : IAdvancedCacheService
{
    protected readonly IDistributedCache _distributedCache;
    protected readonly IConnectionMultiplexer _redis;
    protected readonly ILogger<BaseCacheService> _logger;
    protected readonly JsonSerializerOptions _jsonOptions;

    public BaseCacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer redis,
        ILogger<BaseCacheService> logger)
    {
        _distributedCache = distributedCache;
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public virtual async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return null;
        }
    }

    public virtual async Task SetAsync<T>(string key, T item, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var serializedItem = JsonSerializer.Serialize(item, _jsonOptions);
            
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }

            await _distributedCache.SetStringAsync(key, serializedItem, options, cancellationToken);
            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public virtual async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public virtual async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            return await database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache key existence: {Key}", key);
            return false;
        }
    }

    public virtual async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            
            var keys = server.Keys(pattern: pattern);
            
            foreach (var key in keys)
            {
                await database.KeyDeleteAsync(key);
            }
            
            _logger.LogDebug("Cache removed for pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
        }
    }

    public virtual async Task IncrementAsync(string key, long value = 1, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            await database.StringIncrementAsync(key, value);
            
            if (expiration.HasValue)
            {
                await database.KeyExpireAsync(key, expiration.Value);
            }
            
            _logger.LogDebug("Counter incremented for key: {Key} by {Value}", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing counter for key: {Key}", key);
        }
    }

    public virtual async Task<long> GetCounterAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            var value = await database.StringGetAsync(key);
            return value.HasValue ? (long)value : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting counter value for key: {Key}", key);
            return 0;
        }
    }

    public virtual async Task SetIfNotExistsAsync<T>(string key, T item, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var database = _redis.GetDatabase();
            var serializedItem = JsonSerializer.Serialize(item, _jsonOptions);
            
            var success = await database.StringSetAsync(key, serializedItem, expiration, When.NotExists);
            
            if (success)
            {
                _logger.LogDebug("Cache set (if not exists) for key: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Cache key already exists: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value (if not exists) for key: {Key}", key);
        }
    }
}