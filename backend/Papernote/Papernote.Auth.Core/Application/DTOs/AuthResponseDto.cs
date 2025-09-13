using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Auth.Core.Application.DTOs;

/// <summary>
/// Authentication response containing JWT tokens and user information
/// </summary>
[SwaggerSchema(
    Title = "Authentication Response",
    Description = "Response model containing JWT access token, refresh token, expiration information and user details"
)]
public record AuthResponseDto(
    [SwaggerSchema("JWT access token for API authentication")]
    string AccessToken,

    [SwaggerSchema("Refresh token for obtaining new access tokens")]
    string RefreshToken,

    [SwaggerSchema("UTC timestamp when the access token expires")]
    DateTime ExpiresAt,

    [SwaggerSchema("User information associated with the authentication")]
    UserDto User
);