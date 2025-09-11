using Papernote.Notes.Core.Application.DTOs;

namespace Papernote.Notes.Core.Application.Interfaces;

/// <summary>
/// Service contract for note operations
/// </summary>
public interface INoteService
{
  Task<NoteDto> CreateNoteAsync(CreateNoteDto createNoteDto, Guid userId, CancellationToken cancellationToken = default);
  Task<NoteDto> UpdateNoteAsync(UpdateNoteDto updateNoteDto, Guid userId, CancellationToken cancellationToken = default);
  Task<NoteDto?> GetNoteByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
  Task<IEnumerable<NoteSummaryDto>> GetUserNotesAsync(Guid userId, CancellationToken cancellationToken = default);
  Task<IEnumerable<NoteSummaryDto>> GetNotesByTagAsync(string tag, Guid userId, CancellationToken cancellationToken = default);
  Task DeleteNoteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
  Task<int> GetUserNoteCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
