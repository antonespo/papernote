using Papernote.Notes.Core.Application.DTOs;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.Core.Application.Interfaces;

/// <summary>
/// Service contract for note operations
/// </summary>
public interface INoteService
{
    Task<Result<NoteDto>> CreateNoteAsync(CreateNoteDto createNoteDto, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> UpdateNoteAsync(UpdateNoteDto updateNoteDto, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<NoteSummaryDto>>> GetNotesAsync(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<NoteSummaryDto>>> GetNotesByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<Result> DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<int>> GetNoteCountAsync(CancellationToken cancellationToken = default);
}
