using System.ComponentModel.DataAnnotations;

namespace Papernote.Auth.Core.Application.DTOs;

public record RegisterDto(
    [Required, StringLength(32, MinimumLength = 3)] string Username,
    [Required, StringLength(100, MinimumLength = 8)] string Password
);