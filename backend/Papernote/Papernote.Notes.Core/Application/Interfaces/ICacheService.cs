using Papernote.SharedMicroservices.Cache;

namespace Papernote.Notes.Core.Application.Interfaces;

/// <summary>
/// Interface for caching operations in Notes service
/// Extends base cache service with Notes-specific operations
/// </summary>
public interface ICacheService : IAdvancedCacheService
{
    /// <summary>
    /// Cache a single note with optimized expiration
    /// </summary>
    Task CacheNoteAsync<T>(string key, T note, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Cache search results with shorter expiration
    /// </summary>
    Task CacheSearchResultsAsync<T>(string key, T results, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Cache notes list with default expiration
    /// </summary>
    Task CacheNotesListAsync<T>(string key, T notesList, CancellationToken cancellationToken = default) where T : class;
}