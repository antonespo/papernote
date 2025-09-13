using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Notes.Core.Application.DTOs;

/// <summary>
/// Data transfer object for creating a new note
/// </summary>
[SwaggerSchema(
    Title = "Create Note Request",
    Description = "Request model for creating a new note with title, content, optional tags and initial sharing configuration"
)]
public record CreateNoteDto(
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [SwaggerSchema("Title of the note")]
    string Title,
    
    [Required]
    [StringLength(50000, MinimumLength = 1)]
    [SwaggerSchema("Content of the note in plain text or markdown")]
    string Content,
    
    [SwaggerSchema("Optional list of tags to categorize the note")]
    List<string>? Tags = null,
    
    [SwaggerSchema("Optional list of usernames to share the note with initially")]
    List<string>? SharedWithUsernames = null
);

/// <summary>
/// Data transfer object for updating an existing note
/// </summary>
[SwaggerSchema(
    Title = "Update Note Request", 
    Description = "Request model for updating an existing note's content, tags and sharing configuration"
)]
public record UpdateNoteDto(
    [Required]
    [SwaggerSchema("Unique identifier of the note to update")]
    Guid Id,
    
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [SwaggerSchema("Updated title of the note")]
    string Title,
    
    [Required]
    [StringLength(50000, MinimumLength = 1)]
    [SwaggerSchema("Updated content of the note")]
    string Content,
    
    [SwaggerSchema("Updated list of tags (replaces existing tags)")]
    List<string>? Tags = null,
    
    [SwaggerSchema("Updated list of usernames to share with (replaces existing sharing)")]
    List<string>? SharedWithUsernames = null
);

/// <summary>
/// Data transfer object for searching notes by text content and/or tags
/// </summary>
[SwaggerSchema(
    Title = "Search Notes Request",
    Description = "Request model for searching notes with text and tag filters"
)]
public record SearchNotesDto(
    [SwaggerSchema("Text to search in note titles and content")]
    string? Text = null,
    
    [SwaggerSchema("List of tags to filter by")]
    List<string>? Tags = null
);

/// <summary>
/// Data transfer object for unified note retrieval with filtering and search
/// </summary>
[SwaggerSchema(
    Title = "Get Notes Request",
    Description = "Request model for retrieving notes with filtering and search capabilities"
)]
public record GetNotesDto(
    [SwaggerSchema("Filter type for note ownership")]
    NoteFilter Filter = NoteFilter.Owned,
    
    [SwaggerSchema("Text to search in note titles and content")]
    string? SearchText = null,
    
    [SwaggerSchema("List of tags to filter by")]
    List<string>? SearchTags = null
)
{
    /// <summary>
    /// Indicates if this is a search operation
    /// </summary>
    [SwaggerSchema("Indicates whether this request includes search parameters")]
    public bool IsSearch => !string.IsNullOrWhiteSpace(SearchText) || (SearchTags?.Count > 0);
}

/// <summary>
/// Data transfer object for complete note response
/// </summary>
[SwaggerSchema(
    Title = "Note Details",
    Description = "Complete note information including content, metadata, tags and sharing details"
)]
public class NoteDto
{
    [SwaggerSchema("Unique identifier of the note")]
    public Guid Id { get; set; }
    
    [SwaggerSchema("Title of the note")]
    public string Title { get; set; } = string.Empty;
    
    [SwaggerSchema("Full content of the note")]
    public string Content { get; set; } = string.Empty;
    
    [SwaggerSchema("Date and time when the note was created")]
    public DateTime CreatedAt { get; set; }
    
    [SwaggerSchema("Date and time when the note was last updated")]
    public DateTime UpdatedAt { get; set; }
    
    [SwaggerSchema("List of tags associated with the note")]
    public List<string> Tags { get; set; } = new();
    
    [SwaggerSchema("Username of the note owner")]
    public string OwnerUsername { get; set; } = string.Empty;
    
    [SwaggerSchema("List of usernames the note is shared with")]
    public List<string> SharedWithUsernames { get; set; } = new();
}

/// <summary>
/// Data transfer object for note summary in listing operations
/// </summary>
[SwaggerSchema(
    Title = "Note Summary",
    Description = "Condensed note information for list views with preview content"
)]
public class NoteSummaryDto
{
    [SwaggerSchema("Unique identifier of the note")]
    public Guid Id { get; set; }
    
    [SwaggerSchema("Title of the note")]
    public string Title { get; set; } = string.Empty;
    
    [SwaggerSchema("Preview of the note content (truncated)")]
    public string ContentPreview { get; set; } = string.Empty;
    
    [SwaggerSchema("Date and time when the note was created")]
    public DateTime CreatedAt { get; set; }
    
    [SwaggerSchema("Date and time when the note was last updated")]
    public DateTime UpdatedAt { get; set; }
    
    [SwaggerSchema("List of tags associated with the note")]
    public List<string> Tags { get; set; } = new();
    
    [SwaggerSchema("Username of the note owner")]
    public string OwnerUsername { get; set; } = string.Empty;
}
