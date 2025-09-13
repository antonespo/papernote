using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Auth.Core.Application.DTOs;

/// <summary>
/// User registration data transfer object
/// </summary>
[SwaggerSchema(
    Title = "User Registration Request",
    Description = "Request model for registering a new user account with username and password"
)]
public record RegisterDto(
    [Required]
    [StringLength(32, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, hyphens and underscores")]
    [SwaggerSchema("Username for the new account (3-32 characters, alphanumeric with dots, hyphens and underscores)")]
    string Username,
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    [SwaggerSchema("Password for the new account (8-100 characters, will be securely hashed)")]
    string Password
);