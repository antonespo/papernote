using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Notes.Core.Application.DTOs;

/// <summary>
/// Filter options for note retrieval based on ownership and sharing
/// </summary>
[SwaggerSchema(Description = "Filter options for retrieving notes based on ownership and sharing status")]
public enum NoteFilter
{
    /// <summary>
    /// Notes owned by the current user
    /// </summary>
    Owned,
    
    /// <summary>
    /// Notes shared with the current user by other users
    /// </summary>
    Shared
}

/// <summary>
/// Extension methods for NoteFilter enum operations
/// </summary>
public static class NoteFilterExtensions
{
    /// <summary>
    /// Parses a string value to a NoteFilter enum value
    /// </summary>
    /// <param name="filter">String representation of the filter</param>
    /// <returns>Corresponding NoteFilter value or null if invalid</returns>
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