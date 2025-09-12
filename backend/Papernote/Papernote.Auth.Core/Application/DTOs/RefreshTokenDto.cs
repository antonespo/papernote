using System.ComponentModel.DataAnnotations;

namespace Papernote.Auth.Core.Application.DTOs;

public record RefreshTokenDto(
    [Required] string RefreshToken
);