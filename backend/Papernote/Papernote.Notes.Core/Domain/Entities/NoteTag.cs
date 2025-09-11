namespace Papernote.Notes.Core.Domain.Entities;

public class NoteTag
{
    public Guid NoteId { get; private set; }
    public string TagName { get; private set; } = string.Empty;
    public DateTime AddedAt { get; private set; }

    public Note Note { get; private set; } = null!;

    private NoteTag() { }

    public NoteTag(Guid noteId, string tagName)
    {
        NoteId = noteId;
        TagName = ValidateAndNormalizeTagName(tagName);
        AddedAt = DateTime.UtcNow;
    }

    private static string ValidateAndNormalizeTagName(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be empty", nameof(tagName));
        if (tagName.Length > 50)
            throw new ArgumentException("Tag name cannot exceed 50 characters", nameof(tagName));
        
        return tagName.ToLowerInvariant().Trim();
    }
}