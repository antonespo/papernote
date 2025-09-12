namespace Papernote.Notes.Core.Application.Interfaces;

public interface ICurrentUserService
{
    Guid GetCurrentUserId();

    bool IsAuthenticated { get; }
}