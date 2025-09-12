namespace Papernote.Auth.Core.Application.DTOs;

public record UserResolutionDto(
    Guid UserId,
    string Username
);

public record BatchUserResolutionRequestDto(
    IEnumerable<string> Usernames
);

public record BatchUserIdResolutionRequestDto(
    IEnumerable<Guid> UserIds
);