namespace Papernote.Notes.Core.Application.DTOs;

/// <summary>
/// DTO for creating a new note
/// </summary>
public record CreateNoteDto(
    string Title,
    string Content,
    List<string>? Tags = null
);

/// <summary>
/// DTO for updating an existing note
/// </summary>
public record UpdateNoteDto(
    Guid Id,
    string Title,
    string Content,
    List<string>? Tags = null
);

/// <summary>
/// DTO for note response
/// </summary>
public record NoteDto(
    Guid Id,
    string Title,
    string Content,
    Guid UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<string> Tags
);

/// <summary>
/// DTO for note summary (for listing)
/// </summary>
public record NoteSummaryDto(
    Guid Id,
    string Title,
    string ContentPreview,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<string> Tags
);
