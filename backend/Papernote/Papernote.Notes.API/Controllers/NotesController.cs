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

    [HttpGet("{id:guid}")]
    [ProducesResponseType<NoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _noteService.GetNoteByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<NoteSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserNotes(CancellationToken cancellationToken)
    {
        var result = await _noteService.GetNotesAsync(cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [ProducesResponseType<IEnumerable<NoteSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchNotes(
        [FromQuery] string? text = null,
        [FromQuery] string? tags = null,
        CancellationToken cancellationToken = default)
    {
        var tagList = !string.IsNullOrWhiteSpace(tags)
            ? tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : null;

        var searchDto = new SearchNotesDto(text, tagList);
        var result = await _noteService.SearchNotesAsync(searchDto, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType<NoteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _noteService.DeleteNoteAsync(id, cancellationToken);
        return result.ToActionResult();
    }
}
