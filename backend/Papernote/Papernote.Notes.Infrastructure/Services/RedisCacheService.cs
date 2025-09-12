using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Cache;
using StackExchange.Redis;

namespace Papernote.Notes.Infrastructure.Services;

public class RedisCacheService : BaseCacheService, ICacheService
{
    private const int DEFAULT_CACHE_MINUTES = 30;
    private const int SEARCH_CACHE_MINUTES = 15;
    private const int NOTE_CACHE_MINUTES = 60;

    public RedisCacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
        : base(distributedCache, redis, logger)
    {
    }

    public async Task CacheNoteAsync<T>(string key, T note, CancellationToken cancellationToken = default) where T : class
    {
        await SetAsync(key, note, TimeSpan.FromMinutes(NOTE_CACHE_MINUTES), cancellationToken);
    }

    public async Task CacheSearchResultsAsync<T>(string key, T results, CancellationToken cancellationToken = default) where T : class
    {
        await SetAsync(key, results, TimeSpan.FromMinutes(SEARCH_CACHE_MINUTES), cancellationToken);
    }

    public async Task CacheNotesListAsync<T>(string key, T notesList, CancellationToken cancellationToken = default) where T : class
    {
        await SetAsync(key, notesList, TimeSpan.FromMinutes(DEFAULT_CACHE_MINUTES), cancellationToken);
    }
}