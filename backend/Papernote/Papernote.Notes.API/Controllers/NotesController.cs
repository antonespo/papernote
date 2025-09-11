using Microsoft.AspNetCore.Mvc;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.API.Extensions;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.API.Controllers;

[Route("api/v1/notes")]
public class NotesController : ApiControllerBase
{
    private readonly INoteService _noteService;

    public NotesController(INoteService noteService)
    {
        _noteService = noteService;
    }

    /// <summary>
    /// Retrieves a note by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the note</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The note if found, otherwise a 404 Not Found</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _noteService.GetNoteByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Retrieves all notes for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of note summaries</returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<NoteSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserNotes(CancellationToken cancellationToken)
    {
        var result = await _noteService.GetNotesAsync(cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new note
    /// </summary>
    /// <param name="request">The note creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created note</returns>
    [HttpPost]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateNoteDto request, CancellationToken cancellationToken)
    {
        // Validate model state
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _noteService.CreateNoteAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates an existing note
    /// </summary>
    /// <param name="id">The unique identifier of the note to update</param>
    /// <param name="request">The note update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated note</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteDto request, CancellationToken cancellationToken)
    {
        // Validate ID match
        var idValidationError = ControllerExtensions.ValidateIdMatch(id, request.Id);
        if (idValidationError != null)
            return idValidationError.ToActionResult();

        // Validate model state
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _noteService.UpdateNoteAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a note
    /// </summary>
    /// <param name="id">The unique identifier of the note to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _noteService.DeleteNoteAsync(id, cancellationToken);
        return result.ToActionResult();
    }
}
