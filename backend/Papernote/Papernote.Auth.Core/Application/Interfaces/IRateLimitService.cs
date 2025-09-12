using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.Core.Application.Interfaces;

public interface IRateLimitService
{
    Task<Result<RateLimitCheckResult>> CheckAttemptAsync(string username, string operation, CancellationToken cancellationToken = default);
    Task<Result> RecordFailedAttemptAsync(string username, string operation, CancellationToken cancellationToken = default);
    Task<Result> ClearAttemptsAsync(string username, string operation, CancellationToken cancellationToken = default);
}

public record RateLimitCheckResult(bool IsAllowed, TimeSpan? RetryAfter = null, int CurrentAttempts = 0);