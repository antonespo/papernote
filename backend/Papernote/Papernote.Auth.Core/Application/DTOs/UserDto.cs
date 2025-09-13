using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Auth.Core.Application.DTOs;

/// <summary>
/// User information data transfer object
/// </summary>
[SwaggerSchema(
    Title = "User Information",
    Description = "User profile information including account details and activity timestamps"
)]
public record UserDto(
    [SwaggerSchema("Unique user identifier")]
    Guid Id,

    [SwaggerSchema("Username")]
    string Username,

    [SwaggerSchema("Date and time when the account was created")]
    DateTime CreatedAt,

    [SwaggerSchema("Date and time of the last successful login (null if never logged in)")]
    DateTime? LastLoginAt
);