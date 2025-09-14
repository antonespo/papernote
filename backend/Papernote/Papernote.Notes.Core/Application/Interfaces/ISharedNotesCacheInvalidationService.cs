using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Core.Application.Interfaces;

public interface ISharedNotesCacheInvalidationService
{
    Task InvalidateSharedNotesCacheForUsersAsync(
        IEnumerable<Guid> userIds, 
        CancellationToken cancellationToken = default);

    Task InvalidateSharedNotesCacheForNoteAsync(
        Note note, 
        CancellationToken cancellationToken = default);

    Task InvalidateSharedNotesCacheForNoteUpdateAsync(
        Note oldNote, 
        Note updatedNote, 
        CancellationToken cancellationToken = default);
}