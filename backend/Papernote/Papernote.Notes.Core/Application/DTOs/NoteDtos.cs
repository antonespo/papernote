namespace Papernote.Notes.Core.Application.DTOs;

/// <summary>
/// DTO for creating a new note
/// </summary>
public record CreateNoteDto(
    string Title,
    string Content,
    List<string>? Tags = null,
    List<string>? SharedWithUsernames = null
);

/// <summary>
/// DTO for updating an existing note
/// </summary>
public record UpdateNoteDto(
    Guid Id,
    string Title,
    string Content,
    List<string>? Tags = null,
    List<string>? SharedWithUsernames = null
);

/// <summary>
/// DTO for searching notes by text content and/or tags
/// </summary>
public record SearchNotesDto(
    string? Text = null,
    List<string>? Tags = null
);

/// <summary>
/// DTO for unified note retrieval with filtering and search
/// </summary>
public record GetNotesDto(
    NoteFilter Filter = NoteFilter.Owned,
    string? SearchText = null,
    List<string>? SearchTags = null
)
{
    /// <summary>
    /// Indicates if this is a search operation
    /// </summary>
    public bool IsSearch => !string.IsNullOrWhiteSpace(SearchText) || (SearchTags?.Count > 0);
}

/// <summary>
/// DTO for note response
/// </summary>
public class NoteDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public string OwnerUsername { get; set; } = string.Empty;
    public List<string> SharedWithUsernames { get; set; } = new();
}

/// <summary>
/// DTO for note summary (for listing)
/// </summary>
public class NoteSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentPreview { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public string OwnerUsername { get; set; } = string.Empty;
}
