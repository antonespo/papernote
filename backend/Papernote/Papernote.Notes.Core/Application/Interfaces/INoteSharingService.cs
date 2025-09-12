using Papernote.Notes.Core.Application.DTOs;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Notes.Core.Application.Interfaces;

public interface INoteSharingService
{
    Task<Result> AddNoteShareAsync(
        Guid noteId,
        AddNoteShareRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveNoteShareAsync(
        Guid noteId,
        RemoveNoteShareRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> CanCurrentUserWriteNoteAsync(
        Guid noteId,
        CancellationToken cancellationToken = default);
}