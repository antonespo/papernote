using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.Core.Application.Interfaces;

public interface IUserResolutionService
{
    Task<Result<Dictionary<string, Guid>>> GetUserIdsBatchAsync(IEnumerable<string> usernames, CancellationToken cancellationToken = default);
    Task<Result<Dictionary<Guid, string>>> GetUsernamesBatchAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}