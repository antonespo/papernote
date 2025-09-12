namespace Papernote.Notes.Core.Application.DTOs;

public enum NoteFilter
{
    Owned,
    Shared
}

public static class NoteFilterExtensions
{
    public static NoteFilter? ParseFilter(string? filter)
    {
        return filter?.ToLowerInvariant() switch
        {
            "owned" or null or "" => NoteFilter.Owned,
            "shared" => NoteFilter.Shared,
            _ => null
        };
    }
}