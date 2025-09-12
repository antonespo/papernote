using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Cache;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Papernote.Notes.Infrastructure.Services;

public class NotesCacheKeyStrategy : INotesCacheKeyStrategy, IAdvancedCacheKeyStrategy
{
    public string ServicePrefix => "notes";
    public string Version => "v1";

    public string GetUserNoteKey(Guid userId, Guid noteId)
        => GetVersionedKey("user", userId.ToString(), "note", noteId.ToString());

    public string GetUserNotesListKey(Guid userId, NoteFilter filter)
        => GetVersionedKey("user", userId.ToString(), "list", filter.ToString().ToLowerInvariant());

    public string GetUserSearchKey(Guid userId, NoteFilter filter, string? searchText, IEnumerable<string>? tags)
    {
        var searchQuery = new
        {
            Filter = filter.ToString().ToLowerInvariant(),
            Text = searchText?.Trim().ToLowerInvariant(),
            Tags = tags?.Select(t => t.ToLowerInvariant()).OrderBy(t => t).ToArray()
        };

        var json = JsonSerializer.Serialize(searchQuery);
        var hash = GenerateHash(json);

        return GetVersionedKey("user", userId.ToString(), "search", hash);
    }

    public string GetAllUserCachePattern(Guid userId)
        => GetPatternKey("user", userId.ToString() + ":*");

    public string GetSearchPatternKey()
        => GetPatternKey("user", "*:search:*");

    public string GetPatternKey(string operation, string wildcard = "*")
        => $"{ServicePrefix}:{Version}:{operation}:{wildcard}";

    public string GetVersionedKey(string operation, params string[] segments)
        => $"{ServicePrefix}:{Version}:{operation}:{string.Join(":", segments)}";

    private static string GenerateHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes)[..16];
    }
}