using Papernote.Notes.Core.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Papernote.Notes.Infrastructure.Services;

public class NotesCacheKeyStrategy : ICacheKeyStrategy
{
    private const string NOTE_PREFIX = "notes";
    private const string ID_NAMESPACE = "id";
    private const string SEARCH_NAMESPACE = "search";
    private const string LIST_NAMESPACE = "list";

    public string GetNoteKey(Guid noteId)
    {
        return $"{NOTE_PREFIX}:{ID_NAMESPACE}:{noteId}";
    }

    public string GetSearchKey(string? searchText, IEnumerable<string>? tags)
    {
        var searchQuery = new
        {
            Text = searchText?.Trim().ToLowerInvariant(),
            Tags = tags?.Select(t => t.ToLowerInvariant()).OrderBy(t => t).ToArray()
        };

        var json = JsonSerializer.Serialize(searchQuery);
        var hash = GenerateHash(json);
        
        return $"{NOTE_PREFIX}:{SEARCH_NAMESPACE}:{hash}";
    }

    public string GetNotesListKey()
    {
        return $"{NOTE_PREFIX}:{LIST_NAMESPACE}";
    }

    public string GetAllNotesCachePattern()
    {
        return $"{NOTE_PREFIX}:*";
    }

    private static string GenerateHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes)[..16];
    }
}