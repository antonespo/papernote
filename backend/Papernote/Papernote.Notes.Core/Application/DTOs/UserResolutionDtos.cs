namespace Papernote.Notes.Core.Application.DTOs;

public record ResolveUsernamesToIdsRequest(
    IEnumerable<string> Usernames
);

public record ResolveUsernamesToIdsResponse(
    IDictionary<string, Guid> UserResolutions
);

public record ResolveIdsToUsernamesRequest(
    IEnumerable<Guid> UserIds
);

public record ResolveIdsToUsernamesResponse(
    IDictionary<Guid, string> UserResolutions
);