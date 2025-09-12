namespace Papernote.Notes.Core.Domain.Entities;

public class Note
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public List<NoteTag> NoteTags { get; private set; } = new();
    public List<NoteShare> NoteShares { get; private set; } = new();

    private Note() { }

    public Note(string title, string content, Guid ownerUserId, IEnumerable<string>? tags = null)
    {
        Id = Guid.NewGuid();
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        OwnerUserId = ownerUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;
        NoteTags = tags?.Select(tag => new NoteTag(Id, tag)).ToList() ?? new();
        NoteShares = new();
    }

    public void UpdateContent(string title, string content)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTags(IEnumerable<string> tags)
    {
        NoteTags.Clear();
        NoteTags.AddRange(tags.Select(tag => new NoteTag(Id, tag)));
        UpdatedAt = DateTime.UtcNow;
    }

    public void ShareWith(Guid readerUserId)
    {
        if (readerUserId == OwnerUserId)
            throw new InvalidOperationException("Cannot share note with owner");

        if (IsSharedWith(readerUserId))
            return;

        NoteShares.Add(new NoteShare(Id, readerUserId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveShare(Guid readerUserId)
    {
        var share = NoteShares.FirstOrDefault(ns => ns.ReaderUserId == readerUserId);
        if (share != null)
        {
            NoteShares.Remove(share);
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

    public bool IsOwnedBy(Guid userId) => OwnerUserId == userId;
    public bool IsSharedWith(Guid userId) => NoteShares.Any(ns => ns.ReaderUserId == userId);
    public bool CanBeReadBy(Guid userId) => IsOwnedBy(userId) || IsSharedWith(userId);
    public bool CanBeWrittenBy(Guid userId) => IsOwnedBy(userId);

    public IEnumerable<string> GetTagNames() => NoteTags.Select(nt => nt.TagName);
    public bool HasTag(string tagName) => NoteTags.Any(nt => nt.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase));
    public IEnumerable<Guid> GetSharedUserIds() => NoteShares.Select(ns => ns.ReaderUserId);
}
