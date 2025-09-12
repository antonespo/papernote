using Papernote.Notes.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Cache;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Papernote.Notes.Infrastructure.Services;

public class NotesCacheKeyStrategy : ICacheKeyStrategy, IAdvancedCacheKeyStrategy
{
    public string ServicePrefix => "notes";
    public string Version => "v1";

    public string GetNoteKey(Guid noteId)
        => GetVersionedKey("note", "id", noteId.ToString());

    public string GetSearchKey(string? searchText, IEnumerable<string>? tags)
    {
        var searchQuery = new
        {
            Text = searchText?.Trim().ToLowerInvariant(),
            Tags = tags?.Select(t => t.ToLowerInvariant()).OrderBy(t => t).ToArray()
        };

        var json = JsonSerializer.Serialize(searchQuery);
        var hash = GenerateHash(json);

        return GetVersionedKey("search", hash);
    }

    public string GetNotesListKey()
        => GetVersionedKey("list", "all");

    public string GetAllNotesCachePattern()
        => GetPatternKey("*");

    public string GetPatternKey(string operation, string wildcard = "*")
        => $"{ServicePrefix}:{Version}:{operation}:{wildcard}";

    public string GetVersionedKey(string operation, params string[] segments)
        => $"{ServicePrefix}:{Version}:{operation}:{string.Join(":", segments)}";

    public string GetSearchPatternKey()
        => GetPatternKey("search");

    private static string GenerateHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes)[..16];
    }
}