using Papernote.Notes.Core.Application.DTOs;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.Core.Application.Interfaces;

/// <summary>
/// Service contract for note operations with user context
/// </summary>
public interface INoteService
{
    Task<Result<IEnumerable<NoteSummaryDto>>> GetNotesAsync(GetNotesDto request, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> CreateNoteAsync(CreateNoteDto createNoteDto, CancellationToken cancellationToken = default);
    Task<Result<NoteDto>> UpdateNoteAsync(UpdateNoteDto updateNoteDto, CancellationToken cancellationToken = default);
    Task<Result> DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default);
}
