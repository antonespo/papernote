using Microsoft.Extensions.Logging;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.Infrastructure.Services;

public class CachedNoteService : ICachedNoteService
{
    private readonly INoteService _noteService;
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyStrategy _keyStrategy;
    private readonly ILogger<CachedNoteService> _logger;

    public CachedNoteService(
        INoteService noteService,
        ICacheService cacheService,
        ICacheKeyStrategy keyStrategy,
        ILogger<CachedNoteService> logger)
    {
        _noteService = noteService;
        _cacheService = cacheService;
        _keyStrategy = keyStrategy;
        _logger = logger;
    }

    public async Task<Result<NoteDto>> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = _keyStrategy.GetNoteKey(id);
        
        try
        {
            var cachedNote = await _cacheService.GetAsync<NoteDto>(cacheKey, cancellationToken);
            if (cachedNote != null)
            {
                _logger.LogDebug("Cache HIT for note {NoteId}", id);
                return ResultBuilder.Success(cachedNote);
            }

            _logger.LogDebug("Cache MISS for note {NoteId}", id);

            var result = await _noteService.GetNoteByIdAsync(id, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                await _cacheService.CacheNoteAsync(cacheKey, result.Value, cancellationToken);
                _logger.LogDebug("Cached note {NoteId}", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cached GetNoteByIdAsync for {NoteId}", id);
            return await _noteService.GetNoteByIdAsync(id, cancellationToken);
        }
    }

    public async Task<Result<IEnumerable<NoteSummaryDto>>> GetNotesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = _keyStrategy.GetNotesListKey();
        
        try
        {
            var cachedNotes = await _cacheService.GetAsync<IEnumerable<NoteSummaryDto>>(cacheKey, cancellationToken);
            if (cachedNotes != null)
            {
                _logger.LogDebug("Cache HIT for notes list");
                return ResultBuilder.Success(cachedNotes);
            }

            _logger.LogDebug("Cache MISS for notes list");

            var result = await _noteService.GetNotesAsync(cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                await _cacheService.CacheNotesListAsync(cacheKey, result.Value, cancellationToken);
                _logger.LogDebug("Cached notes list");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cached GetNotesAsync");
            return await _noteService.GetNotesAsync(cancellationToken);
        }
    }

    public async Task<Result<IEnumerable<NoteSummaryDto>>> SearchNotesAsync(SearchNotesDto searchDto, CancellationToken cancellationToken = default)
    {
        var cacheKey = _keyStrategy.GetSearchKey(searchDto.Text, searchDto.Tags);
        
        try
        {
            var cachedResults = await _cacheService.GetAsync<IEnumerable<NoteSummaryDto>>(cacheKey, cancellationToken);
            if (cachedResults != null)
            {
                _logger.LogDebug("Cache HIT for search query");
                return ResultBuilder.Success(cachedResults);
            }

            _logger.LogDebug("Cache MISS for search query");

            var result = await _noteService.SearchNotesAsync(searchDto, cancellationToken);
            
            if (result.IsSuccess && result.Value != null)
            {
                await _cacheService.CacheSearchResultsAsync(cacheKey, result.Value, cancellationToken);
                _logger.LogDebug("Cached search results");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cached SearchNotesAsync");
            return await _noteService.SearchNotesAsync(searchDto, cancellationToken);
        }
    }

    public async Task<Result<NoteDto>> CreateNoteAsync(CreateNoteDto createNoteDto, CancellationToken cancellationToken = default)
    {
        var result = await _noteService.CreateNoteAsync(createNoteDto, cancellationToken);
        
        if (result.IsSuccess)
        {
            await InvalidateNotesListCacheAsync(cancellationToken);
            await InvalidateSearchCachesAsync(cancellationToken);
            _logger.LogDebug("Invalidated caches after note creation");
        }

        return result;
    }

    public async Task<Result<NoteDto>> UpdateNoteAsync(UpdateNoteDto updateNoteDto, CancellationToken cancellationToken = default)
    {
        var result = await _noteService.UpdateNoteAsync(updateNoteDto, cancellationToken);
        
        if (result.IsSuccess)
        {
            await InvalidateNoteCacheAsync(updateNoteDto.Id, cancellationToken);
            await InvalidateNotesListCacheAsync(cancellationToken);
            await InvalidateSearchCachesAsync(cancellationToken);
            _logger.LogDebug("Invalidated caches after note update {NoteId}", updateNoteDto.Id);
        }

        return result;
    }

    public async Task<Result> DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _noteService.DeleteNoteAsync(id, cancellationToken);
        
        if (result.IsSuccess)
        {
            await InvalidateNoteCacheAsync(id, cancellationToken);
            await InvalidateNotesListCacheAsync(cancellationToken);
            await InvalidateSearchCachesAsync(cancellationToken);
            _logger.LogDebug("Invalidated caches after note deletion {NoteId}", id);
        }

        return result;
    }

    public async Task InvalidateNoteCacheAsync(Guid noteId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = _keyStrategy.GetNoteKey(noteId);
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Invalidated cache for note {NoteId}", noteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for note {NoteId}", noteId);
        }
    }

    public async Task InvalidateSearchCachesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = "notes:search:*";
            await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
            _logger.LogDebug("Invalidated all search caches");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating search caches");
        }
    }

    public async Task InvalidateNotesListCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = _keyStrategy.GetNotesListKey();
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Invalidated notes list cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating notes list cache");
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
                    var cacheKey = _keyStrategy.GetNoteKey(noteId);
                    await _cacheService.CacheNoteAsync(cacheKey, result.Value, cancellationToken);
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