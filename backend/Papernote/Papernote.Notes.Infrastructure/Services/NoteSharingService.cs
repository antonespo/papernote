using Microsoft.Extensions.Logging;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Core.Domain.Interfaces;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.Infrastructure.Services;

public class NoteSharingService : INoteSharingService
{
    private readonly INoteRepository _noteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthUserResolutionService _userResolutionService;
    private readonly ICachedNoteService _cachedNoteService;
    private readonly ILogger<NoteSharingService> _logger;

    public NoteSharingService(
        INoteRepository noteRepository,
        ICurrentUserService currentUserService,
        IAuthUserResolutionService userResolutionService,
        ICachedNoteService cachedNoteService,
        ILogger<NoteSharingService> logger)
    {
        _noteRepository = noteRepository;
        _currentUserService = currentUserService;
        _userResolutionService = userResolutionService;
        _cachedNoteService = cachedNoteService;
        _logger = logger;
    }

    public async Task<Result> AddNoteShareAsync(
        Guid noteId,
        AddNoteShareRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();

            if (!await CanCurrentUserWriteNoteAsync(noteId, cancellationToken))
            {
                return ResultBuilder.Forbidden("You don't have permission to share this note");
            }

            var targetUserId = await _userResolutionService.ResolveUsernameToIdAsync(
                request.Username, cancellationToken);

            if (!targetUserId.HasValue)
            {
                return ResultBuilder.NotFound($"User '{request.Username}' not found");
            }

            if (targetUserId.Value == currentUserId)
            {
                return ResultBuilder.BadRequest("Cannot share note with yourself");
            }

            var note = await _noteRepository.GetByIdWithTagsAndSharesAsync(noteId, cancellationToken);
            if (note == null)
            {
                return ResultBuilder.NotFound($"Note with ID {noteId} not found");
            }

            if (note.IsSharedWith(targetUserId.Value))
            {
                return ResultBuilder.Conflict($"Note is already shared with user '{request.Username}'");
            }

            note.ShareWith(targetUserId.Value);
            await _noteRepository.UpdateAsync(note, cancellationToken);

            await _cachedNoteService.InvalidateNoteCacheAsync(noteId, cancellationToken);

            _logger.LogInformation("Note {NoteId} shared with user {Username} by {CurrentUserId}",
                noteId, request.Username, currentUserId);

            return ResultBuilder.Success();
        }
        catch (UnauthorizedAccessException)
        {
            return ResultBuilder.Unauthorized("Authentication required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add share for note {NoteId} with user {Username}",
                noteId, request.Username);
            return ResultBuilder.InternalServerError("Failed to share note");
        }
    }

    public async Task<Result> RemoveNoteShareAsync(
        Guid noteId,
        RemoveNoteShareRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();

            if (!await CanCurrentUserWriteNoteAsync(noteId, cancellationToken))
            {
                return ResultBuilder.Forbidden("You don't have permission to modify shares for this note");
            }

            var targetUserId = await _userResolutionService.ResolveUsernameToIdAsync(
                request.Username, cancellationToken);

            if (!targetUserId.HasValue)
            {
                return ResultBuilder.NotFound($"User '{request.Username}' not found");
            }

            var note = await _noteRepository.GetByIdWithTagsAndSharesAsync(noteId, cancellationToken);
            if (note == null)
            {
                return ResultBuilder.NotFound($"Note with ID {noteId} not found");
            }

            if (!note.IsSharedWith(targetUserId.Value))
            {
                return ResultBuilder.NotFound($"Note is not shared with user '{request.Username}'");
            }

            note.RemoveShare(targetUserId.Value);
            await _noteRepository.UpdateAsync(note, cancellationToken);

            await _cachedNoteService.InvalidateNoteCacheAsync(noteId, cancellationToken);

            _logger.LogInformation("Share removed for note {NoteId} with user {Username} by {CurrentUserId}",
                noteId, request.Username, currentUserId);

            return ResultBuilder.Success();
        }
        catch (UnauthorizedAccessException)
        {
            return ResultBuilder.Unauthorized("Authentication required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove share for note {NoteId} with user {Username}",
                noteId, request.Username);
            return ResultBuilder.InternalServerError("Failed to remove note share");
        }
    }

    public async Task<bool> CanCurrentUserWriteNoteAsync(
        Guid noteId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            return await _noteRepository.CanUserWriteNoteAsync(noteId, currentUserId, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check write permission for note {NoteId}", noteId);
            return false;
        }
    }
}