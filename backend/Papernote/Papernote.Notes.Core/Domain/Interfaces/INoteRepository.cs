using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Core.Domain.Interfaces;

/// <summary>
/// Repository contract for Note entity
/// </summary>
public interface INoteRepository
{
  Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<IEnumerable<Note>> GetByUserIdAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default);
  Task<IEnumerable<Note>> GetByTagAsync(string tag, Guid userId, CancellationToken cancellationToken = default);
  Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default);
  Task<Note> UpdateAsync(Note note, CancellationToken cancellationToken = default);
  Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
  Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
  Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
