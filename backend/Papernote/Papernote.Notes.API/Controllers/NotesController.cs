using Microsoft.AspNetCore.Mvc;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var note = await _noteService.GetNoteByIdAsync(id, cancellationToken);

        if (note == null)
            return ResultBuilder.Failure("Note not found", "NotFound").ToActionResult();

        return ResultBuilder.Success(note).ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetUserNotes(CancellationToken cancellationToken)
    {
        var notes = await _noteService.GetNotesAsync(cancellationToken);
        return ResultBuilder.Success(notes).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNoteDto request, CancellationToken cancellationToken)
    {
        var note = await _noteService.CreateNoteAsync(request, cancellationToken);
        return ResultBuilder.Success(note).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNoteDto request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return ResultBuilder.Failure("Id mismatch", "BadRequest").ToActionResult();

        var note = await _noteService.UpdateNoteAsync(request, cancellationToken);
        return ResultBuilder.Success(note).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _noteService.DeleteNoteAsync(id, cancellationToken);
        return ResultBuilder.Success().ToActionResult();
    }
}
