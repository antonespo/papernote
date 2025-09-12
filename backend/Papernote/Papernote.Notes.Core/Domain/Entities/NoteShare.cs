namespace Papernote.Notes.Core.Domain.Entities;

public class NoteShare
{
    public Guid NoteId { get; private set; }
    public Guid ReaderUserId { get; private set; }
    public DateTime SharedAt { get; private set; }

    public Note Note { get; private set; } = null!;

    private NoteShare() { }

    public NoteShare(Guid noteId, Guid readerUserId)
    {
        NoteId = noteId;
        ReaderUserId = readerUserId;
        SharedAt = DateTime.UtcNow;
    }
}