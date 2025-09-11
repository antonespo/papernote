using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Core.Domain.Interfaces;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Note?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default);
    Task<Note> UpdateAsync(Note note, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> SearchAsync(string searchText, CancellationToken cancellationToken = default);
}
