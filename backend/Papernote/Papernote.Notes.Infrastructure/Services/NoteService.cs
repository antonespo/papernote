using AutoMapper;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Core.Domain.Entities;
using Papernote.Notes.Core.Domain.Interfaces;

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

    public async Task<NoteDto> CreateNoteAsync(CreateNoteDto createNoteDto, CancellationToken cancellationToken = default)
    {
        var note = _mapper.Map<Note>(createNoteDto);
        var created = await _noteRepository.CreateAsync(note, cancellationToken);
        return _mapper.Map<NoteDto>(created);
    }

    public async Task<NoteDto?> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await _noteRepository.GetByIdWithTagsAsync(id, cancellationToken);
        if (note == null)
            return null;
        return _mapper.Map<NoteDto>(note);
    }

    public async Task<IEnumerable<NoteSummaryDto>> GetNotesAsync(CancellationToken cancellationToken = default)
    {
        var notes = await _noteRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<NoteSummaryDto>>(notes);
    }

    public async Task<NoteDto> UpdateNoteAsync(UpdateNoteDto updateNoteDto, CancellationToken cancellationToken = default)
    {
        var note = await _noteRepository.GetByIdWithTagsAsync(updateNoteDto.Id, cancellationToken);
        if (note == null)
            throw new InvalidOperationException("Note not found");

        note.UpdateContent(updateNoteDto.Title, updateNoteDto.Content);

        if (updateNoteDto.Tags != null)
        {
            note.UpdateTags(updateNoteDto.Tags);
        }

        var updated = await _noteRepository.UpdateAsync(note, cancellationToken);
        return _mapper.Map<NoteDto>(updated);
    }

    public async Task<IEnumerable<NoteSummaryDto>> GetNotesByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        var notes = await _noteRepository.GetByTagAsync(tag, cancellationToken);
        return _mapper.Map<IEnumerable<NoteSummaryDto>>(notes);
    }

    public async Task DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _noteRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<int> GetNoteCountAsync(CancellationToken cancellationToken = default)
    {
        return await _noteRepository.GetCountAsync(cancellationToken);
    }
}