namespace Papernote.Notes.Core.Domain.Entities;

/// <summary>
/// Represents a note sharing relationship between users
/// </summary>
public class NoteShare
{
    public Guid Id { get; private set; }
    public Guid NoteId { get; private set; }
    public Guid SharedWithUserId { get; private set; }
    public Guid SharedByUserId { get; private set; }
    public DateTime SharedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    // Navigation properties
    public Note Note { get; private set; } = null!;

    // Private constructor for EF Core
    private NoteShare() { }

    public NoteShare(Guid noteId, Guid sharedWithUserId, Guid sharedByUserId)
    {
        Id = Guid.NewGuid();
        NoteId = noteId;
        SharedWithUserId = sharedWithUserId;
        SharedByUserId = sharedByUserId;
        SharedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public void Restore()
    {
        IsRevoked = false;
    }
}