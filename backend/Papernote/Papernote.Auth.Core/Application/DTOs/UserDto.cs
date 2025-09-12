namespace Papernote.Auth.Core.Application.DTOs;

public record UserDto(
    Guid Id,
    string Username,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);