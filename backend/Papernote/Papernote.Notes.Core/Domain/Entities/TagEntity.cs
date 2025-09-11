namespace Papernote.Notes.Core.Domain.Entities;

/// <summary>
/// Represents a unique tag in the system
/// </summary>
public class TagEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public List<NoteTag> NoteTags { get; private set; } = new();

    private TagEntity() { }

    public TagEntity(string name)
    {
        Id = Guid.NewGuid();
        Name = ValidateAndNormalizeName(name);
        CreatedAt = DateTime.UtcNow;
    }

    private static string ValidateAndNormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty", nameof(name));
        if (name.Length > 32)
            throw new ArgumentException("Tag name cannot exceed 32 characters", nameof(name));
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-zA-Z0-9_-]+$"))
            throw new ArgumentException("Tag name contains invalid characters", nameof(name));

        return name.ToLowerInvariant();
    }
}