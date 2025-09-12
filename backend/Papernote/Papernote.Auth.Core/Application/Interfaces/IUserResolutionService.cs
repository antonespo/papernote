using Papernote.Auth.Core.Application.DTOs;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.Core.Application.Interfaces;

public interface IUserResolutionService
{
    Task<Result<Guid?>> GetUserIdByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Result<string?>> GetUsernameByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<Dictionary<string, Guid>>> GetUserIdsBatchAsync(IEnumerable<string> usernames, CancellationToken cancellationToken = default);
    Task<Result<Dictionary<Guid, string>>> GetUsernamesBatchAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}