using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Auth.Core.Application.DTOs;

/// <summary>
/// Refresh token request for obtaining new JWT tokens
/// </summary>
[SwaggerSchema(
    Title = "Token Refresh Request",
    Description = "Request model containing a refresh token for obtaining new JWT access and refresh tokens"
)]
public record RefreshTokenDto(
    [Required]
    [SwaggerSchema("Valid refresh token to exchange for new JWT tokens")]
    string RefreshToken
);