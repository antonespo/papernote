using Papernote.Notes.Core.Application.DTOs;

namespace Papernote.Notes.Core.Application.Interfaces;

public interface INotesCacheKeyStrategy
{
    string GetUserNoteKey(Guid userId, Guid noteId);
    string GetUserNotesListKey(Guid userId, NoteFilter filter);
    string GetUserSearchKey(Guid userId, NoteFilter filter, string? searchText, IEnumerable<string>? tags);
    string GetAllUserCachePattern(Guid userId);
    string GetSearchPatternKey();
    string GetPatternKey(string operation, string wildcard = "*");
}