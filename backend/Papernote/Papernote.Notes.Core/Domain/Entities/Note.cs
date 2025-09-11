namespace Papernote.Notes.Core.Domain.Entities;

/// <summary>
/// Represents a note in the system
/// </summary>
public class Note
{
  public Guid Id { get; private set; }
  public string Title { get; private set; } = string.Empty;
  public string Content { get; private set; } = string.Empty;
  public Guid UserId { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }
  public bool IsDeleted { get; private set; }

  public List<NoteTag> NoteTags { get; private set; } = new();
  public List<NoteShare> NoteShares { get; private set; } = new();

  // Private constructor for EF Core
  private Note() { }

  public Note(string title, string content, Guid userId)
  {
    Id = Guid.NewGuid();
    Title = title ?? throw new ArgumentNullException(nameof(title));
    Content = content ?? throw new ArgumentNullException(nameof(content));
    UserId = userId;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
    IsDeleted = false;
    NoteTags = new List<NoteTag>();
    NoteShares = new List<NoteShare>();
  }

  public void UpdateContent(string title, string content)
  {
    Title = title ?? throw new ArgumentNullException(nameof(title));
    Content = content ?? throw new ArgumentNullException(nameof(content));
    UpdatedAt = DateTime.UtcNow;
  }

  public void MarkAsDeleted()
  {
    IsDeleted = true;
    UpdatedAt = DateTime.UtcNow;
  }

  public void Restore()
  {
    IsDeleted = false;
    UpdatedAt = DateTime.UtcNow;
  }

  public IEnumerable<TagEntity> GetTags() => NoteTags.Select(nt => nt.Tag);
  public bool HasTag(string tagName) => NoteTags.Any(nt => nt.Tag.Name == tagName.ToLowerInvariant());
}
