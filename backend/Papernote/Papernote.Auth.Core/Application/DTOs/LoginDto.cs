using System.ComponentModel.DataAnnotations;

namespace Papernote.Auth.Core.Application.DTOs;

public record LoginDto(
    [Required] string Username,
    [Required] string Password
);