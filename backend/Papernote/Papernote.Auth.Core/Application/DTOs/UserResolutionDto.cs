using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Auth.Core.Application.DTOs;

/// <summary>
/// User resolution data transfer object containing user ID and username mapping
/// </summary>
[SwaggerSchema(
    Title = "User Resolution",
    Description = "Mapping between user ID and username for internal microservice communication"
)]
public record UserResolutionDto(
    [SwaggerSchema("Unique user identifier")]
    Guid UserId,
    
    [SwaggerSchema("Username")]
    string Username
);

/// <summary>
/// Batch request for resolving multiple usernames to user IDs
/// </summary>
[SwaggerSchema(
    Title = "Batch Username Resolution Request",
    Description = "Request containing multiple usernames to resolve to their corresponding user IDs"
)]
public record BatchUserResolutionRequestDto(
    [Required]
    [SwaggerSchema("Collection of usernames to resolve")]
    IEnumerable<string> Usernames
);

/// <summary>
/// Batch request for resolving multiple user IDs to usernames
/// </summary>
[SwaggerSchema(
    Title = "Batch User ID Resolution Request", 
    Description = "Request containing multiple user IDs to resolve to their corresponding usernames"
)]
public record BatchUserIdResolutionRequestDto(
    [Required]
    [SwaggerSchema("Collection of user IDs to resolve")]
    IEnumerable<Guid> UserIds
);