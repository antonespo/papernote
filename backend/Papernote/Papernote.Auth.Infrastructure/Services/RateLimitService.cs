using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Papernote.Auth.Core.Application.Configuration;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Cache;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.Infrastructure.Services;

public class RateLimitService : IRateLimitService
{
    private readonly IAdvancedCacheService _cacheService;
    private readonly IAuthCacheKeyStrategy _keyStrategy;
    private readonly RateLimitSettings _settings;
    private readonly ILogger<RateLimitService> _logger;

    public RateLimitService(
        IAdvancedCacheService cacheService,
        IAuthCacheKeyStrategy keyStrategy,
        IOptions<RateLimitSettings> settings,
        ILogger<RateLimitService> logger)
    {
        _cacheService = cacheService;
        _keyStrategy = keyStrategy;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<RateLimitCheckResult>> CheckAttemptAsync(string username, string operation, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || operation?.ToLowerInvariant() != "login")
            return ResultBuilder.Success(new RateLimitCheckResult(true));

        var normalizedUsername = username.ToLowerInvariant();
        var attemptsKey = _keyStrategy.GetRateLimitKey(normalizedUsername);

        try
        {
            var currentAttempts = await _cacheService.GetCounterAsync(attemptsKey, cancellationToken);

            if (currentAttempts >= _settings.MaxAttempts)
            {
                var windowDuration = TimeSpan.FromMinutes(_settings.WindowMinutes);

                _logger.LogWarning("Rate limit exceeded for username: {Username}, attempts: {Attempts}",
                    normalizedUsername, currentAttempts);

                return ResultBuilder.Success(new RateLimitCheckResult(false, windowDuration, (int)currentAttempts));
            }

            return ResultBuilder.Success(new RateLimitCheckResult(true, null, (int)currentAttempts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for username: {Username}", normalizedUsername);
            // Fail open for availability
            return ResultBuilder.Success(new RateLimitCheckResult(true));
        }
    }

    public async Task<Result> RecordFailedAttemptAsync(string username, string operation, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || operation?.ToLowerInvariant() != "login")
            return ResultBuilder.Success();

        var normalizedUsername = username.ToLowerInvariant();
        var attemptsKey = _keyStrategy.GetRateLimitKey(normalizedUsername);
        var windowDuration = TimeSpan.FromMinutes(_settings.WindowMinutes);

        try
        {
            await _cacheService.IncrementAsync(attemptsKey, 1, windowDuration, cancellationToken);

            var currentAttempts = await _cacheService.GetCounterAsync(attemptsKey, cancellationToken);
            _logger.LogInformation("Recorded failed login attempt for username: {Username}, total attempts: {Attempts}",
                normalizedUsername, currentAttempts);

            return ResultBuilder.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording failed attempt for username: {Username}", normalizedUsername);
            return ResultBuilder.InternalServerError("Failed to record rate limit attempt");
        }
    }

    public async Task<Result> ClearAttemptsAsync(string username, string operation, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || operation?.ToLowerInvariant() != "login")
            return ResultBuilder.Success();

        var normalizedUsername = username.ToLowerInvariant();
        var attemptsKey = _keyStrategy.GetRateLimitKey(normalizedUsername);

        try
        {
            await _cacheService.RemoveAsync(attemptsKey, cancellationToken);

            _logger.LogInformation("Cleared rate limit attempts for username: {Username}", normalizedUsername);

            return ResultBuilder.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing attempts for username: {Username}", normalizedUsername);
            return ResultBuilder.InternalServerError("Failed to clear rate limit attempts");
        }
    }
}