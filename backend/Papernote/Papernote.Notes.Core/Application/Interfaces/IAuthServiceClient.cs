using Papernote.Notes.Core.Application.DTOs;
using Refit;

namespace Papernote.Notes.Core.Application.Interfaces;

/// <summary>
/// Refit client interface for Auth service communication
/// </summary>
public interface IAuthServiceClient
{
    [Post("/api/internal/users/resolve/batch/usernames")]
    Task<Dictionary<string, Guid>> ResolveUsernamesToIdsAsync(
        [Body] ResolveUsernamesToIdsRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/internal/users/resolve/batch/userids")]
    Task<Dictionary<Guid, string>> ResolveIdsToUsernamesAsync(
        [Body] ResolveIdsToUsernamesRequest request,
        CancellationToken cancellationToken = default);
}