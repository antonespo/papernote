using System.ComponentModel.DataAnnotations;

namespace Papernote.Auth.Core.Application.DTOs;

public record LogoutDto(
    [Required] Guid UserId
);

public record RevokeAllTokensDto(
    [Required] Guid UserId
);