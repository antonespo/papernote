using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Papernote.Notes.API.Extensions;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Results;
using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Notes.API.Controllers;

/// <summary>
/// Manages note operations including CRUD operations, sharing and search functionality
/// </summary>
[Authorize]
[Route("api/v1/notes")]
[ApiController]
[SwaggerTag("Note management operations including creation, retrieval, updates, deletion and sharing")]
public class NotesController : ApiControllerBase
{
    private readonly INoteService _noteService;

    public NotesController(INoteService noteService)
    {
        _noteService = noteService;
    }

    /// <summary>
    /// Retrieves a specific note by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the note</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>The complete note details including content, tags and sharing information</returns>
    /// <response code="200">Note retrieved successfully</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission to access this note</response>
    /// <response code="404">Note not found</response>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Get note by ID",
        Description = "Retrieves a complete note with all details including content, tags, owner information and sharing details. User must be either the owner or have read access through sharing.",
        OperationId = "GetNoteById",
        Tags = new[] { "Notes" }
    )]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        [SwaggerParameter("Unique identifier of the note", Required = true)] Guid id, 
        CancellationToken cancellationToken)
    {
        var result = await _noteService.GetNoteByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Retrieves a filtered and searchable list of notes
    /// </summary>
    /// <param name="filter">Filter to apply: 'owned' for user's own notes, 'shared' for notes shared with the user</param>
    /// <param name="text">Optional text to search within note titles and content using full-text search</param>
    /// <param name="tags">Optional comma-separated list of tags to filter by</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>A list of note summaries matching the specified criteria</returns>
    /// <response code="200">Notes retrieved successfully</response>
    /// <response code="400">Invalid filter parameter provided</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get filtered list of notes",
        Description = "Retrieves a list of note summaries with optional filtering and full-text search capabilities. Supports filtering by ownership (owned/shared) and searching by text content and tags.",
        OperationId = "GetNotes",
        Tags = new[] { "Notes" }
    )]
    [ProducesResponseType<IEnumerable<NoteSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotes(
        [FromQuery, SwaggerParameter("Filter type: 'owned' for user's notes, 'shared' for shared notes")] string? filter = null,
        [FromQuery, SwaggerParameter("Text to search in note titles and content")] string? text = null,
        [FromQuery, SwaggerParameter("Comma-separated list of tags to filter by")] string? tags = null,
        CancellationToken cancellationToken = default)
    {
        var filterEnum = NoteFilterExtensions.ParseFilter(filter);
        if (filterEnum == null)
        {
            return BadRequest("Invalid filter parameter. Use 'owned' or 'shared'.");
        }

        var tagList = !string.IsNullOrWhiteSpace(tags)
            ? tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : null;

        var request = new GetNotesDto(
            Filter: filterEnum.Value,
            SearchText: text,
            SearchTags: tagList);

        var result = await _noteService.GetNotesAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new note with optional tags and sharing
    /// </summary>
    /// <param name="request">The note creation data including title, content, tags and initial sharing settings</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>The created note with its assigned unique identifier</returns>
    /// <response code="201">Note created successfully</response>
    /// <response code="400">Invalid request data provided</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="422">Validation errors in the request data</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new note",
        Description = "Creates a new note with the provided title, content, optional tags and initial sharing configuration. The authenticated user becomes the owner of the note.",
        OperationId = "CreateNote",
        Tags = new[] { "Notes" }
    )]
    [ProducesResponseType<NoteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody, SwaggerRequestBody("Note creation data")] CreateNoteDto request, 
        CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _noteService.CreateNoteAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value!.Id },
                result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Updates an existing note's content, tags and sharing settings
    /// </summary>
    /// <param name="id">The unique identifier of the note to update</param>
    /// <param name="request">The updated note data including title, content, tags and sharing configuration</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>The updated note with all current details</returns>
    /// <response code="200">Note updated successfully</response>
    /// <response code="400">Invalid request data or ID mismatch</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission to modify this note</response>
    /// <response code="404">Note not found</response>
    /// <response code="422">Validation errors in the request data</response>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Update an existing note",
        Description = "Updates a note's title, content, tags and sharing configuration. Only the note owner can perform updates. Changes to sharing will add/remove users as specified.",
        OperationId = "UpdateNote",
        Tags = new[] { "Notes" }
    )]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        [SwaggerParameter("Unique identifier of the note to update", Required = true)] Guid id, 
        [FromBody, SwaggerRequestBody("Updated note data")] UpdateNoteDto request, 
        CancellationToken cancellationToken)
    {
        var idValidationError = ControllerExtensions.ValidateIdMatch(id, request.Id);
        if (idValidationError != null)
            return idValidationError.ToActionResult();

        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _noteService.UpdateNoteAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Soft deletes a note and all its associated data
    /// </summary>
    /// <param name="id">The unique identifier of the note to delete</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Note deleted successfully</response>
    /// <response code="400">Invalid note identifier provided</response>
    /// <response code="401">User is not authenticated</response>
    /// <response code="403">User does not have permission to delete this note</response>
    /// <response code="404">Note not found</response>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Delete a note",
        Description = "Soft deletes a note by marking it as deleted rather than removing it from the database. Only the note owner can delete a note. The note will no longer appear in listings but can potentially be restored.",
        OperationId = "DeleteNote",
        Tags = new[] { "Notes" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        [SwaggerParameter("Unique identifier of the note to delete", Required = true)] Guid id, 
        CancellationToken cancellationToken)
    {
        var result = await _noteService.DeleteNoteAsync(id, cancellationToken);
        return result.ToActionResult();
    }
}
