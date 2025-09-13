using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Core.Domain.Interfaces;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Note?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Note?> GetByIdWithTagsAndSharesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> GetNotesAsync(GetNotesDto request, Guid userId, CancellationToken cancellationToken = default);
    Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default);
    Task<Note> UpdateAsync(Note note, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CanUserReadNoteAsync(Guid noteId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanUserWriteNoteAsync(Guid noteId, Guid userId, CancellationToken cancellationToken = default);
}
