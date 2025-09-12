namespace Papernote.Notes.Core.Application.DTOs;

public record AddNoteShareRequest(
    string Username
);

public record RemoveNoteShareRequest(
    string Username
);

public record NoteShareDto(
    Guid NoteId,
    Guid ReaderUserId,
    DateTime SharedAt
);