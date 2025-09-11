namespace Papernote.Notes.Core.Domain.Entities;

/// <summary>
/// Value Object per rappresentare un Tag associato a una nota
/// </summary>
public readonly record struct Tag
{
    public string Value { get; }

    public Tag(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tag cannot be empty", nameof(value));
        if (value.Length > 32)
            throw new ArgumentException("Tag cannot exceed 32 characters", nameof(value));
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, "^[a-zA-Z0-9_-]+$"))
            throw new ArgumentException("Tag contains invalid characters", nameof(value));
        Value = value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}
