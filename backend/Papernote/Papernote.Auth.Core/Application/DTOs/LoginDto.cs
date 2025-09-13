using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Auth.Core.Application.DTOs;

/// <summary>
/// User login credentials data transfer object
/// </summary>
[SwaggerSchema(
    Title = "User Login Request",
    Description = "Request model for user authentication with username and password credentials"
)]
public record LoginDto(
    [Required]
    [SwaggerSchema("Username or email address")]
    string Username,
    
    [Required]
    [SwaggerSchema("User password")]
    string Password
);