using AutoMapper;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Core.Domain.Entities;
using Papernote.Notes.Core.Domain.Interfaces;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.Infrastructure.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly IMapper _mapper;

    public NoteService(INoteRepository noteRepository, IMapper mapper)
    {
        _noteRepository = noteRepository;
        _mapper = mapper;
    }

    public async Task<Result<NoteDto>> CreateNoteAsync(CreateNoteDto createNoteDto, CancellationToken cancellationToken = default)
    {
        if (createNoteDto is null)
            return ResultBuilder.BadRequest<NoteDto>("Request cannot be null");

        if (string.IsNullOrWhiteSpace(createNoteDto.Title))
            return ResultBuilder.ValidationError<NoteDto>("Title is required");

        if (string.IsNullOrWhiteSpace(createNoteDto.Content))
            return ResultBuilder.ValidationError<NoteDto>("Content is required");

        try
        {
            var note = _mapper.Map<Note>(createNoteDto);
            var created = await _noteRepository.CreateAsync(note, cancellationToken);
            var noteDto = _mapper.Map<NoteDto>(created);
            return ResultBuilder.Success(noteDto);
        }
        catch (Exception ex)
        {
            return ResultBuilder.InternalServerError<NoteDto>($"Failed to create note: {ex.Message}");
        }
    }

    public async Task<Result<NoteDto>> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return ResultBuilder.BadRequest<NoteDto>("Invalid note ID");

        try
        {
            var note = await _noteRepository.GetByIdWithTagsAsync(id, cancellationToken);
            if (note == null)
                return ResultBuilder.NotFound<NoteDto>($"Note with ID {id} not found");

            var noteDto = _mapper.Map<NoteDto>(note);
            return ResultBuilder.Success(noteDto);
        }
        catch (Exception ex)
        {
            return ResultBuilder.InternalServerError<NoteDto>($"Failed to retrieve note: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<NoteSummaryDto>>> GetNotesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var notes = await _noteRepository.GetAllAsync(cancellationToken);
            var noteDtos = _mapper.Map<IEnumerable<NoteSummaryDto>>(notes);
            return ResultBuilder.Success(noteDtos);
        }
        catch (Exception ex)
        {
            return ResultBuilder.InternalServerError<IEnumerable<NoteSummaryDto>>($"Failed to retrieve notes: {ex.Message}");
        }
    }

    public async Task<Result<NoteDto>> UpdateNoteAsync(UpdateNoteDto updateNoteDto, CancellationToken cancellationToken = default)
    {
        if (updateNoteDto is null)
            return ResultBuilder.BadRequest<NoteDto>("Request cannot be null");

        if (updateNoteDto.Id == Guid.Empty)
            return ResultBuilder.BadRequest<NoteDto>("Invalid note ID");

        if (string.IsNullOrWhiteSpace(updateNoteDto.Title))
            return ResultBuilder.ValidationError<NoteDto>("Title is required");

        if (string.IsNullOrWhiteSpace(updateNoteDto.Content))
            return ResultBuilder.ValidationError<NoteDto>("Content is required");

        try
        {
            var note = await _noteRepository.GetByIdWithTagsAsync(updateNoteDto.Id, cancellationToken);
            if (note == null)
                return ResultBuilder.NotFound<NoteDto>($"Note with ID {updateNoteDto.Id} not found");

            note.UpdateContent(updateNoteDto.Title, updateNoteDto.Content);

            if (updateNoteDto.Tags != null)
            {
                note.UpdateTags(updateNoteDto.Tags);
            }

            var updated = await _noteRepository.UpdateAsync(note, cancellationToken);
            var noteDto = _mapper.Map<NoteDto>(updated);
            return ResultBuilder.Success(noteDto);
        }
        catch (Exception ex)
        {
            return ResultBuilder.InternalServerError<NoteDto>($"Failed to update note: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<NoteSummaryDto>>> SearchNotesAsync(SearchNotesDto searchDto, CancellationToken cancellationToken = default)
    {
        if (searchDto is null)
            return ResultBuilder.BadRequest<IEnumerable<NoteSummaryDto>>("Search request cannot be null");

        if (string.IsNullOrWhiteSpace(searchDto.Text) && (searchDto.Tags == null || searchDto.Tags.Count == 0))
            return ResultBuilder.BadRequest<IEnumerable<NoteSummaryDto>>("At least one search parameter (text or tags) must be provided");

        try
        {
            var notes = await _noteRepository.SearchNotesAsync(searchDto.Text, searchDto.Tags, cancellationToken);
            var noteDtos = _mapper.Map<IEnumerable<NoteSummaryDto>>(notes);
            return ResultBuilder.Success(noteDtos);
        }
        catch (Exception ex)
        {
            return ResultBuilder.InternalServerError<IEnumerable<NoteSummaryDto>>($"Failed to search notes: {ex.Message}");
        }
    }

    public async Task<Result> DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return ResultBuilder.BadRequest("Invalid note ID");

        try
        {
            var exists = await _noteRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
                return ResultBuilder.NotFound($"Note with ID {id} not found");

            await _noteRepository.DeleteAsync(id, cancellationToken);
            return ResultBuilder.Success();
        }
        catch (Exception ex)
        {
            return ResultBuilder.InternalServerError($"Failed to delete note: {ex.Message}");
        }
    }
}