using Microsoft.Extensions.Logging;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.Auth.Core.Domain.Interfaces;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.Infrastructure.Services;

public class UserResolutionService : IUserResolutionService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserResolutionService> _logger;

    public UserResolutionService(
        IUserRepository userRepository,
        ILogger<UserResolutionService> logger)
    {
        _userRepository = userRepository;
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
            var userIdsByUsername = await _userRepository.GetUserIdsByUsernamesAsync(usernamesList, cancellationToken);

            _logger.LogDebug("Resolved {Count} users from {RequestedCount} usernames", 
                userIdsByUsername.Count, usernamesList.Count);

            return ResultBuilder.Success(userIdsByUsername);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch user ID resolution for {Count} usernames", usernamesList.Count);
            return ResultBuilder.InternalServerError<Dictionary<string, Guid>>("Batch user resolution failed");
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
            var usernamesByUserId = await _userRepository.GetUsernamesByUserIdsAsync(userIdsList, cancellationToken);

            _logger.LogDebug("Resolved {Count} usernames from {RequestedCount} user IDs", 
                usernamesByUserId.Count, userIdsList.Count);

            return ResultBuilder.Success(usernamesByUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch username resolution for {Count} user IDs", userIdsList.Count);
            return ResultBuilder.InternalServerError<Dictionary<Guid, string>>("Batch username resolution failed");
        }
    }
}