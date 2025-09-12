using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Papernote.Notes.API.Extensions;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.API.Controllers;

[Authorize]
[Route("api/v1/notes")]
public class NotesController : ApiControllerBase
{
    private readonly INoteService _noteService;
    private readonly INoteSharingService _noteSharingService;

    public NotesController(INoteService noteService, INoteSharingService noteSharingService)
    {
        _noteService = noteService;
        _noteSharingService = noteSharingService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _noteService.GetNoteByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<NoteSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotes(
        [FromQuery] string? filter = null,
        [FromQuery] string? text = null,
        [FromQuery] string? tags = null,
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

    [HttpPost]
    [ProducesResponseType<NoteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateNoteDto request, CancellationToken cancellationToken)
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteDto request, CancellationToken cancellationToken)
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

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _noteService.DeleteNoteAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/share")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddShare(Guid id, [FromBody] AddNoteShareRequest request, CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _noteSharingService.AddNoteShareAsync(id, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}/share")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveShare(Guid id, [FromBody] RemoveNoteShareRequest request, CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _noteSharingService.RemoveNoteShareAsync(id, request, cancellationToken);
        return result.ToActionResult();
    }
}
