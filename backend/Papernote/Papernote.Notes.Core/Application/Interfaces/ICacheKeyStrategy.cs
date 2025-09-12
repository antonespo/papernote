namespace Papernote.Notes.Core.Application.Interfaces;

public interface ICacheKeyStrategy
{
    string GetNoteKey(Guid noteId);
    string GetSearchKey(string? searchText, IEnumerable<string>? tags);
    string GetNotesListKey();
    string GetAllNotesCachePattern();
}