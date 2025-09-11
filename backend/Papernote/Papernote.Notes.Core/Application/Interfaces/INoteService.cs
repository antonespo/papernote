using Papernote.Notes.Core.Application.DTOs;

namespace Papernote.Notes.Core.Application.Interfaces;

/// <summary>
/// Service contract for note operations
/// </summary>
public interface INoteService
{
  Task<NoteDto> CreateNoteAsync(CreateNoteDto createNoteDto, CancellationToken cancellationToken = default);
  Task<NoteDto> UpdateNoteAsync(UpdateNoteDto updateNoteDto, CancellationToken cancellationToken = default);
  Task<NoteDto?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<IEnumerable<NoteSummaryDto>> GetNotesAsync(CancellationToken cancellationToken = default);
  Task<IEnumerable<NoteSummaryDto>> GetNotesByTagAsync(string tag, CancellationToken cancellationToken = default);
  Task DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default);
  Task<int> GetNoteCountAsync(CancellationToken cancellationToken = default);
}
