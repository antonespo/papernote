namespace Papernote.Notes.Core.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between Notes and Tags
/// </summary>
public class NoteTag
{
    public Guid NoteId { get; private set; }
    public Guid TagId { get; private set; }
    public DateTime AddedAt { get; private set; }

    public Note Note { get; private set; } = null!;
    public TagEntity Tag { get; private set; } = null!;

    private NoteTag() { }

    public NoteTag(Guid noteId, Guid tagId)
    {
        NoteId = noteId;
        TagId = tagId;
        AddedAt = DateTime.UtcNow;
    }
}