using Microsoft.Extensions.Logging;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Infrastructure.Services;

public class SharedNotesCacheInvalidationService : ISharedNotesCacheInvalidationService
{
    private readonly ICacheService _cacheService;
    private readonly INotesCacheKeyStrategy _keyStrategy;
    private readonly ILogger<SharedNotesCacheInvalidationService> _logger;

    public SharedNotesCacheInvalidationService(
        ICacheService cacheService,
        INotesCacheKeyStrategy keyStrategy,
        ILogger<SharedNotesCacheInvalidationService> logger)
    {
        _cacheService = cacheService;
        _keyStrategy = keyStrategy;
        _logger = logger;
    }

    public async Task InvalidateSharedNotesCacheForUsersAsync(
        IEnumerable<Guid> userIds, 
        CancellationToken cancellationToken = default)
    {
        if (!userIds?.Any() == true)
            return;

        try
        {
            var invalidationTasks = userIds.Select(async userId =>
            {
                var sharedNotesListKey = _keyStrategy.GetUserNotesListKey(userId, NoteFilter.Shared);
                var userSearchPattern = _keyStrategy.GetPatternKey("user", $"{userId}:search:*");

                await Task.WhenAll(
                    _cacheService.RemoveAsync(sharedNotesListKey, cancellationToken),
                    _cacheService.RemoveByPatternAsync(userSearchPattern, cancellationToken)
                );

                _logger.LogDebug("Invalidated shared notes cache for user {UserId}", userId);
            });

            await Task.WhenAll(invalidationTasks);

            _logger.LogDebug("Invalidated shared notes cache for {Count} users", userIds.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating shared notes cache for users");
        }
    }

    public async Task InvalidateSharedNotesCacheForNoteAsync(
        Note note, 
        CancellationToken cancellationToken = default)
    {
        if (note?.NoteShares?.Any() != true)
            return;

        var sharedUserIds = note.GetSharedUserIds();
        await InvalidateSharedNotesCacheForUsersAsync(sharedUserIds, cancellationToken);
    }

    public async Task InvalidateSharedNotesCacheForNoteUpdateAsync(
        Note oldNote, 
        Note updatedNote, 
        CancellationToken cancellationToken = default)
    {
        var oldSharedUserIds = oldNote?.GetSharedUserIds()?.ToHashSet() ?? new HashSet<Guid>();
        var newSharedUserIds = updatedNote?.GetSharedUserIds()?.ToHashSet() ?? new HashSet<Guid>();

        var allAffectedUserIds = oldSharedUserIds.Union(newSharedUserIds);

        if (allAffectedUserIds.Any())
        {
            await InvalidateSharedNotesCacheForUsersAsync(allAffectedUserIds, cancellationToken);
            
            _logger.LogDebug("Invalidated shared notes cache after note update for {Count} users (old: {OldCount}, new: {NewCount})", 
                allAffectedUserIds.Count(), oldSharedUserIds.Count, newSharedUserIds.Count);
        }
    }
}