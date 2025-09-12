using Microsoft.Extensions.Logging;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.Auth.Infrastructure.Services;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.Infrastructure.Services;

public class CachedUserResolutionService : ICachedUserResolutionService
{
    private readonly IUserResolutionService _userResolutionService;
    private readonly AuthCacheService _cacheService;
    private readonly IAuthCacheKeyStrategy _keyStrategy;
    private readonly ILogger<CachedUserResolutionService> _logger;

    public CachedUserResolutionService(
        IUserResolutionService userResolutionService,
        AuthCacheService cacheService,
        IAuthCacheKeyStrategy keyStrategy,
        ILogger<CachedUserResolutionService> logger)
    {
        _userResolutionService = userResolutionService;
        _cacheService = cacheService;
        _keyStrategy = keyStrategy;
        _logger = logger;
    }

    public async Task<Result<Dictionary<string, Guid>>> GetUserIdsBatchAsync(IEnumerable<string> usernames, CancellationToken cancellationToken = default)
    {
        if (usernames == null)
            return ResultBuilder.BadRequest<Dictionary<string, Guid>>("Usernames list is required");

        var usernamesList = usernames.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();

        if (usernamesList.Count == 0)
            return ResultBuilder.Success(new Dictionary<string, Guid>());

        try
        {
            var result = new Dictionary<string, Guid>();
            var uncachedUsernames = new List<string>();

            foreach (var username in usernamesList)
            {
                var cacheKey = _keyStrategy.GetUserResolutionKey(username);
                var cachedUserIdStr = await _cacheService.GetUserResolutionAsync<string>(cacheKey, cancellationToken);

                if (!string.IsNullOrWhiteSpace(cachedUserIdStr) && Guid.TryParse(cachedUserIdStr, out var cachedUserId))
                {
                    result[username] = cachedUserId;
                    _logger.LogDebug("Cache HIT for batch username: {Username}", username);
                }
                else
                {
                    uncachedUsernames.Add(username);
                }
            }

            if (uncachedUsernames.Count > 0)
            {
                _logger.LogDebug("Cache MISS for {Count} usernames in batch", uncachedUsernames.Count);

                var serviceResult = await _userResolutionService.GetUserIdsBatchAsync(uncachedUsernames, cancellationToken);

                if (serviceResult.IsSuccess)
                {
                    foreach (var kvp in serviceResult.Value)
                    {
                        result[kvp.Key] = kvp.Value;
                        var cacheKey = _keyStrategy.GetUserResolutionKey(kvp.Key);
                        await _cacheService.CacheUserResolutionAsync(cacheKey, kvp.Value.ToString(), cancellationToken);
                    }

                    _logger.LogDebug("Cached {Count} username resolutions from batch", serviceResult.Value.Count);
                }
                else
                {
                    return serviceResult;
                }
            }

            _logger.LogDebug("Batch username resolution completed: {Total} total, {Cached} from cache, {Service} from service",
                usernamesList.Count, usernamesList.Count - uncachedUsernames.Count, uncachedUsernames.Count);

            return ResultBuilder.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cached GetUserIdsBatchAsync for {Count} usernames", usernamesList.Count);
            return await _userResolutionService.GetUserIdsBatchAsync(usernames, cancellationToken);
        }
    }

    public async Task<Result<Dictionary<Guid, string>>> GetUsernamesBatchAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds == null)
            return ResultBuilder.BadRequest<Dictionary<Guid, string>>("User IDs list is required");

        var userIdsList = userIds.Where(id => id != Guid.Empty).Distinct().ToList();

        if (userIdsList.Count == 0)
            return ResultBuilder.Success(new Dictionary<Guid, string>());

        try
        {
            var result = new Dictionary<Guid, string>();
            var uncachedUserIds = new List<Guid>();

            foreach (var userId in userIdsList)
            {
                var cacheKey = _keyStrategy.GetUserIdResolutionKey(userId);
                var cachedUsername = await _cacheService.GetUserResolutionAsync<string>(cacheKey, cancellationToken);

                if (!string.IsNullOrWhiteSpace(cachedUsername))
                {
                    result[userId] = cachedUsername;
                    _logger.LogDebug("Cache HIT for batch userId: {UserId}", userId);
                }
                else
                {
                    uncachedUserIds.Add(userId);
                }
            }

            if (uncachedUserIds.Count > 0)
            {
                _logger.LogDebug("Cache MISS for {Count} userIds in batch", uncachedUserIds.Count);

                var serviceResult = await _userResolutionService.GetUsernamesBatchAsync(uncachedUserIds, cancellationToken);

                if (serviceResult.IsSuccess)
                {
                    foreach (var kvp in serviceResult.Value)
                    {
                        result[kvp.Key] = kvp.Value;
                        var cacheKey = _keyStrategy.GetUserIdResolutionKey(kvp.Key);
                        await _cacheService.CacheUserResolutionAsync(cacheKey, kvp.Value, cancellationToken);
                    }

                    _logger.LogDebug("Cached {Count} userId resolutions from batch", serviceResult.Value.Count);
                }
                else
                {
                    return serviceResult;
                }
            }

            _logger.LogDebug("Batch userId resolution completed: {Total} total, {Cached} from cache, {Service} from service",
                userIdsList.Count, userIdsList.Count - uncachedUserIds.Count, uncachedUserIds.Count);

            return ResultBuilder.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cached GetUsernamesBatchAsync for {Count} userIds", userIdsList.Count);
            return await _userResolutionService.GetUsernamesBatchAsync(userIds, cancellationToken);
        }
    }

    public async Task InvalidateUserCacheAsync(string username, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var usernameKey = _keyStrategy.GetUserResolutionKey(username);
            var userIdKey = _keyStrategy.GetUserIdResolutionKey(userId);

            await Task.WhenAll(
                _cacheService.InvalidateUserResolutionAsync(usernameKey, cancellationToken),
                _cacheService.InvalidateUserResolutionAsync(userIdKey, cancellationToken)
            );

            _logger.LogDebug("Invalidated user cache for username: {Username}, userId: {UserId}", username, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating user cache for username: {Username}, userId: {UserId}", username, userId);
        }
    }
}