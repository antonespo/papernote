using Microsoft.Extensions.Logging;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Core.Domain.Interfaces;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.Infrastructure.Services;

public class CachedNoteService : ICachedNoteService
{
    private readonly INoteService _noteService;
    private readonly INoteRepository _noteRepository;
    private readonly ICacheService _cacheService;
    private readonly INotesCacheKeyStrategy _keyStrategy;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISharedNotesCacheInvalidationService _sharedCacheInvalidationService;
    private readonly ILogger<CachedNoteService> _logger;

    public CachedNoteService(
        INoteService noteService,
        INoteRepository noteRepository,
        ICacheService cacheService,
        INotesCacheKeyStrategy keyStrategy,
        ICurrentUserService currentUserService,
        ISharedNotesCacheInvalidationService sharedCacheInvalidationService,
        ILogger<CachedNoteService> logger)
    {
        _noteService = noteService;
        _noteRepository = noteRepository;
        _cacheService = cacheService;
        _keyStrategy = keyStrategy;
        _currentUserService = currentUserService;
        _sharedCacheInvalidationService = sharedCacheInvalidationService;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<NoteSummaryDto>>> GetNotesAsync(GetNotesDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();

        var cacheKey = request.IsSearch
            ? _keyStrategy.GetUserSearchKey(currentUserId, request.Filter, request.SearchText, request.SearchTags)
            : _keyStrategy.GetUserNotesListKey(currentUserId, request.Filter);

        try
        {
            var cachedNotes = await _cacheService.GetAsync<IEnumerable<NoteSummaryDto>>(cacheKey, cancellationToken);
            if (cachedNotes != null)
            {
                _logger.LogDebug("Cache HIT for notes operation - Filter: {Filter}, IsSearch: {IsSearch}, UserId: {UserId}",
                    request.Filter, request.IsSearch, currentUserId);
                return ResultBuilder.Success(cachedNotes);
            }

            _logger.LogDebug("Cache MISS for notes operation - Filter: {Filter}, IsSearch: {IsSearch}, UserId: {UserId}",
                request.Filter, request.IsSearch, currentUserId);

            var result = await _noteService.GetNotesAsync(request, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                var cacheExpiry = request.IsSearch ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(2);
                await _cacheService.SetAsync(cacheKey, result.Value, cacheExpiry, cancellationToken);
                _logger.LogDebug("Cached notes operation - Filter: {Filter}, IsSearch: {IsSearch}, UserId: {UserId}",
                    request.Filter, request.IsSearch, currentUserId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cached GetNotesAsync for Filter: {Filter}, IsSearch: {IsSearch}, UserId: {UserId}",
                request.Filter, request.IsSearch, currentUserId);
            return await _noteService.GetNotesAsync(request, cancellationToken);
        }
    }

    public async Task<Result<NoteDto>> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var cacheKey = _keyStrategy.GetUserNoteKey(currentUserId, id);

        try
        {
            var cachedNote = await _cacheService.GetAsync<NoteDto>(cacheKey, cancellationToken);
            if (cachedNote != null)
            {
                _logger.LogDebug("Cache HIT for note {NoteId} user {UserId}", id, currentUserId);
                return ResultBuilder.Success(cachedNote);
            }

            _logger.LogDebug("Cache MISS for note {NoteId} user {UserId}", id, currentUserId);

            var result = await _noteService.GetNoteByIdAsync(id, cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                await _cacheService.SetAsync(cacheKey, result.Value, TimeSpan.FromMinutes(10), cancellationToken);
                _logger.LogDebug("Cached note {NoteId} for user {UserId}", id, currentUserId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cached GetNoteByIdAsync for {NoteId} user {UserId}", id, currentUserId);
            return await _noteService.GetNoteByIdAsync(id, cancellationToken);
        }
    }

    public async Task<Result<NoteDto>> CreateNoteAsync(CreateNoteDto createNoteDto, CancellationToken cancellationToken = default)
    {
        var result = await _noteService.CreateNoteAsync(createNoteDto, cancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            await InvalidateUserCachesAsync(cancellationToken);

            if (createNoteDto.SharedWithUsernames?.Any() == true)
            {
                var createdNote = await _noteRepository.GetByIdWithTagsAndSharesAsync(
                    result.Value.Id, cancellationToken);
                
                if (createdNote != null)
                {
                    await _sharedCacheInvalidationService.InvalidateSharedNotesCacheForNoteAsync(
                        createdNote, cancellationToken);
                }
            }

            _logger.LogDebug("Invalidated caches after note creation");
        }

        return result;
    }

    public async Task<Result<NoteDto>> UpdateNoteAsync(UpdateNoteDto updateNoteDto, CancellationToken cancellationToken = default)
    {
        var oldNote = await _noteRepository.GetByIdWithTagsAndSharesAsync(
            updateNoteDto.Id, cancellationToken);

        var result = await _noteService.UpdateNoteAsync(updateNoteDto, cancellationToken);

        if (result.IsSuccess)
        {
            var updatedNote = await _noteRepository.GetByIdWithTagsAndSharesAsync(
                updateNoteDto.Id, cancellationToken);

            await InvalidateNoteCacheAsync(updateNoteDto.Id, cancellationToken);
            await InvalidateUserCachesAsync(cancellationToken);

            if (oldNote != null && updatedNote != null)
            {
                await _sharedCacheInvalidationService.InvalidateSharedNotesCacheForNoteUpdateAsync(
                    oldNote, updatedNote, cancellationToken);
            }
            else if (updatedNote != null)
            {
                await _sharedCacheInvalidationService.InvalidateSharedNotesCacheForNoteAsync(
                    updatedNote, cancellationToken);
            }

            _logger.LogDebug("Invalidated caches after note update {NoteId}", updateNoteDto.Id);
        }

        return result;
    }

    public async Task<Result> DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var noteToDelete = await _noteRepository.GetByIdWithTagsAndSharesAsync(id, cancellationToken);

        var result = await _noteService.DeleteNoteAsync(id, cancellationToken);

        if (result.IsSuccess)
        {
            await InvalidateNoteCacheAsync(id, cancellationToken);
            await InvalidateUserCachesAsync(cancellationToken);

            if (noteToDelete != null)
            {
                await _sharedCacheInvalidationService.InvalidateSharedNotesCacheForNoteAsync(
                    noteToDelete, cancellationToken);
            }

            _logger.LogDebug("Invalidated caches after note deletion {NoteId}", id);
        }

        return result;
    }

    public async Task InvalidateNoteCacheAsync(Guid noteId, CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = _keyStrategy.GetPatternKey("user", $"*:note:{noteId}");
            await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
            _logger.LogDebug("Invalidated cache for note {NoteId} all users", noteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for note {NoteId}", noteId);
        }
    }

    public async Task InvalidateUserCachesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var userPattern = _keyStrategy.GetAllUserCachePattern(currentUserId);
            var searchPattern = _keyStrategy.GetSearchPatternKey();

            await Task.WhenAll(
                _cacheService.RemoveByPatternAsync(userPattern, cancellationToken),
                _cacheService.RemoveByPatternAsync(searchPattern, cancellationToken)
            );

            _logger.LogDebug("Invalidated user caches for user {UserId}", currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating user caches");
        }
    }

    public async Task WarmUpCacheAsync(IEnumerable<Guid> noteIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = noteIds.Select(async noteId =>
            {
                var result = await _noteService.GetNoteByIdAsync(noteId, cancellationToken);
                if (result.IsSuccess && result.Value != null)
                {
                    var currentUserId = _currentUserService.GetCurrentUserId();
                    var cacheKey = _keyStrategy.GetUserNoteKey(currentUserId, noteId);
                    await _cacheService.SetAsync(cacheKey, result.Value, TimeSpan.FromMinutes(10), cancellationToken);
                }
            });

            await Task.WhenAll(tasks);
            _logger.LogDebug("Cache warm-up completed for {Count} notes", noteIds.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache warm-up");
        }
    }
}