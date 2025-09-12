using Microsoft.Extensions.Logging;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;

namespace Papernote.Notes.Infrastructure.Services;

public class AuthUserResolutionService : IAuthUserResolutionService
{
    private readonly IAuthServiceClient _authClient;
    private readonly ILogger<AuthUserResolutionService> _logger;

    public AuthUserResolutionService(
        IAuthServiceClient authClient,
        ILogger<AuthUserResolutionService> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    public async Task<ResolveUsernamesToIdsResponse> ResolveUsernamesToIdsAsync(
        ResolveUsernamesToIdsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Resolving {Count} usernames via Auth service", request.Usernames.Count());
            
            var authResponse = await _authClient.ResolveUsernamesToIdsAsync(request, cancellationToken);
            var response = new ResolveUsernamesToIdsResponse(authResponse);
            
            _logger.LogDebug("Successfully resolved {Count} usernames", response.UserResolutions.Count);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve usernames via Auth service");
            return new ResolveUsernamesToIdsResponse(new Dictionary<string, Guid>());
        }
    }

    public async Task<ResolveIdsToUsernamesResponse> ResolveIdsToUsernamesAsync(
        ResolveIdsToUsernamesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Resolving {Count} user IDs via Auth service", request.UserIds.Count());
            
            var authResponse = await _authClient.ResolveIdsToUsernamesAsync(request, cancellationToken);
            var response = new ResolveIdsToUsernamesResponse(authResponse);
            
            _logger.LogDebug("Successfully resolved {Count} user IDs", response.UserResolutions.Count);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve user IDs via Auth service");
            return new ResolveIdsToUsernamesResponse(new Dictionary<Guid, string>());
        }
    }

    public async Task<Guid?> ResolveUsernameToIdAsync(string username, CancellationToken cancellationToken = default)
    {
        var request = new ResolveUsernamesToIdsRequest(new[] { username });
        var response = await ResolveUsernamesToIdsAsync(request, cancellationToken);
        
        return response.UserResolutions.TryGetValue(username, out var userId) ? userId : null;
    }

    public async Task<string?> ResolveIdToUsernameAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var request = new ResolveIdsToUsernamesRequest(new[] { userId });
        var response = await ResolveIdsToUsernamesAsync(request, cancellationToken);
        
        return response.UserResolutions.TryGetValue(userId, out var username) ? username : null;
    }
}