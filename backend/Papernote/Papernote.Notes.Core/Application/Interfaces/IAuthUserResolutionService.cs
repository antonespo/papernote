using Papernote.Notes.Core.Application.DTOs;

namespace Papernote.Notes.Core.Application.Interfaces;

public interface IAuthUserResolutionService
{
    Task<ResolveUsernamesToIdsResponse> ResolveUsernamesToIdsAsync(
        ResolveUsernamesToIdsRequest request,
        CancellationToken cancellationToken = default);

    Task<ResolveIdsToUsernamesResponse> ResolveIdsToUsernamesAsync(
        ResolveIdsToUsernamesRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid?> ResolveUsernameToIdAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<string?> ResolveIdToUsernameAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}