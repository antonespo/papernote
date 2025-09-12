using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.Core.Application.Interfaces;

public interface ICachedUserResolutionService : IUserResolutionService
{
    Task InvalidateUserCacheAsync(string username, Guid userId, CancellationToken cancellationToken = default);
}