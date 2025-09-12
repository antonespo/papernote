using Papernote.Notes.Core.Application.DTOs;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.Core.Application.Interfaces;

public interface ICachedNoteService : INoteService
{
    Task InvalidateNoteCacheAsync(Guid noteId, CancellationToken cancellationToken = default);
    Task InvalidateSearchCachesAsync(CancellationToken cancellationToken = default);
    Task InvalidateNotesListCacheAsync(CancellationToken cancellationToken = default);
    Task WarmUpCacheAsync(IEnumerable<Guid> noteIds, CancellationToken cancellationToken = default);
}