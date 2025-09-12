using AutoMapper;
using Microsoft.Extensions.Logging;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.Notes.Core.Domain.Entities;
using Papernote.Notes.Core.Domain.Interfaces;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.Infrastructure.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthUserResolutionService _userResolutionService;
    private readonly IMapper _mapper;
    private readonly ILogger<NoteService> _logger;

    public NoteService(
        INoteRepository noteRepository,
        ICurrentUserService currentUserService,
        IAuthUserResolutionService userResolutionService,
        IMapper mapper,
        ILogger<NoteService> logger)
    {
        _noteRepository = noteRepository;
        _currentUserService = currentUserService;
        _userResolutionService = userResolutionService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<NoteSummaryDto>>> GetNotesAsync(GetNotesDto request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            return ResultBuilder.BadRequest<IEnumerable<NoteSummaryDto>>("Request cannot be null");

        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();

            if (request.IsSearch)
            {
                if (string.IsNullOrWhiteSpace(request.SearchText) && (request.SearchTags == null || request.SearchTags.Count == 0))
                {
                    return ResultBuilder.BadRequest<IEnumerable<NoteSummaryDto>>("At least one search parameter (text or tags) must be provided");
                }
            }

            var notes = await _noteRepository.GetNotesAsync(request, currentUserId, cancellationToken);
            var noteDtos = await MapNotesToSummaryDtos(notes, cancellationToken);

            return ResultBuilder.Success(noteDtos);
        }
        catch (UnauthorizedAccessException)
        {
            return ResultBuilder.Unauthorized<IEnumerable<NoteSummaryDto>>("Authentication required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve notes with filter {Filter}, search: {IsSearch}",
                request.Filter, request.IsSearch);
            return ResultBuilder.InternalServerError<IEnumerable<NoteSummaryDto>>("Failed to retrieve notes");
        }
    }

    public async Task<Result<NoteDto>> GetNoteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return ResultBuilder.BadRequest<NoteDto>("Invalid note ID");

        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();

            if (!await _noteRepository.CanUserReadNoteAsync(id, currentUserId, cancellationToken))
            {
                return ResultBuilder.Forbidden<NoteDto>("You don't have permission to access this note");
            }

            var note = await _noteRepository.GetByIdWithTagsAndSharesAsync(id, cancellationToken);
            if (note == null)
                return ResultBuilder.NotFound<NoteDto>($"Note with ID {id} not found");

            var noteDto = await MapNoteToDto(note, cancellationToken);
            return ResultBuilder.Success(noteDto);
        }
        catch (UnauthorizedAccessException)
        {
            return ResultBuilder.Unauthorized<NoteDto>("Authentication required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve note {NoteId}", id);
            return ResultBuilder.InternalServerError<NoteDto>("Failed to retrieve note");
        }
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
            var currentUserId = _currentUserService.GetCurrentUserId();
            var note = new Note(createNoteDto.Title, createNoteDto.Content, currentUserId, createNoteDto.Tags);

            var created = await _noteRepository.CreateAsync(note, cancellationToken);
            var noteDto = await MapNoteToDto(created, cancellationToken);

            return ResultBuilder.Success(noteDto);
        }
        catch (UnauthorizedAccessException)
        {
            return ResultBuilder.Unauthorized<NoteDto>("Authentication required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create note");
            return ResultBuilder.InternalServerError<NoteDto>("Failed to create note");
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
            var currentUserId = _currentUserService.GetCurrentUserId();

            if (!await _noteRepository.CanUserWriteNoteAsync(updateNoteDto.Id, currentUserId, cancellationToken))
            {
                return ResultBuilder.Forbidden<NoteDto>("You don't have permission to modify this note");
            }

            var note = await _noteRepository.GetByIdWithTagsAndSharesAsync(updateNoteDto.Id, cancellationToken);
            if (note == null)
                return ResultBuilder.NotFound<NoteDto>($"Note with ID {updateNoteDto.Id} not found");

            note.UpdateContent(updateNoteDto.Title, updateNoteDto.Content);

            if (updateNoteDto.Tags != null)
            {
                note.UpdateTags(updateNoteDto.Tags);
            }

            var updated = await _noteRepository.UpdateAsync(note, cancellationToken);
            var noteDto = await MapNoteToDto(updated, cancellationToken);

            return ResultBuilder.Success(noteDto);
        }
        catch (UnauthorizedAccessException)
        {
            return ResultBuilder.Unauthorized<NoteDto>("Authentication required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update note {NoteId}", updateNoteDto.Id);
            return ResultBuilder.InternalServerError<NoteDto>("Failed to update note");
        }
    }

    public async Task<Result> DeleteNoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return ResultBuilder.BadRequest("Invalid note ID");

        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();

            if (!await _noteRepository.CanUserWriteNoteAsync(id, currentUserId, cancellationToken))
            {
                return ResultBuilder.Forbidden("You don't have permission to delete this note");
            }

            var exists = await _noteRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
                return ResultBuilder.NotFound($"Note with ID {id} not found");

            await _noteRepository.DeleteAsync(id, cancellationToken);
            return ResultBuilder.Success();
        }
        catch (UnauthorizedAccessException)
        {
            return ResultBuilder.Unauthorized("Authentication required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete note {NoteId}", id);
            return ResultBuilder.InternalServerError("Failed to delete note");
        }
    }

    private async Task<NoteDto> MapNoteToDto(Note note, CancellationToken cancellationToken)
    {
        var noteDto = _mapper.Map<NoteDto>(note);

        var ownerUsername = await _userResolutionService.ResolveIdToUsernameAsync(note.OwnerUserId, cancellationToken);
        noteDto.OwnerUsername = ownerUsername ?? "Unknown";

        if (note.NoteShares.Any())
        {
            var sharedUserIds = note.GetSharedUserIds().ToList();
            var resolutionRequest = new ResolveIdsToUsernamesRequest(sharedUserIds);
            var resolutionResponse = await _userResolutionService.ResolveIdsToUsernamesAsync(resolutionRequest, cancellationToken);

            noteDto.SharedWithUsernames = resolutionResponse.UserResolutions.Values.ToList();
        }

        return noteDto;
    }

    private async Task<IEnumerable<NoteSummaryDto>> MapNotesToSummaryDtos(IEnumerable<Note> notes, CancellationToken cancellationToken)
    {
        var notesArray = notes.ToArray();
        if (notesArray.Length == 0)
            return Array.Empty<NoteSummaryDto>();

        var allOwnerIds = notesArray.Select(n => n.OwnerUserId).Distinct().ToList();
        var resolutionRequest = new ResolveIdsToUsernamesRequest(allOwnerIds);
        var resolutionResponse = await _userResolutionService.ResolveIdsToUsernamesAsync(resolutionRequest, cancellationToken);

        return notesArray.Select(note =>
        {
            var summaryDto = _mapper.Map<NoteSummaryDto>(note);
            summaryDto.OwnerUsername = resolutionResponse.UserResolutions.TryGetValue(note.OwnerUserId, out var username)
                ? username
                : "Unknown";
            return summaryDto;
        });
    }
}