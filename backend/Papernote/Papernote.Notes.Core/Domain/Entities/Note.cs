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
    public List<Tag> Tags { get; private set; } = new();

    // Private constructor for EF Core
    private Note() { }

    public Note(string title, string content, Guid userId, IEnumerable<Tag>? tags = null)
    {
        Id = Guid.NewGuid();
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;
        Tags = tags?.Distinct().ToList() ?? new List<Tag>();
    }

    public void UpdateContent(string title, string content)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTag(Tag tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(Tag tag)
    {
        if (Tags.RemoveAll(t => t.Equals(tag)) > 0)
        {
            UpdatedAt = DateTime.UtcNow;
        }
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
}
