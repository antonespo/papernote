using Microsoft.AspNetCore.Mvc;
using Papernote.Auth.API.Extensions;
using Papernote.Auth.Core.Application.DTOs;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.SharedMicroservices.Results;
using Papernote.SharedMicroservices.Security;
using Swashbuckle.AspNetCore.Annotations;

namespace Papernote.Auth.API.Controllers;

/// <summary>
/// Internal API for resolving user identifiers between usernames and user IDs for microservice communication
/// </summary>
[Route("api/internal/users")]
[ApiController]
[InternalApiKey]
[SwaggerTag("Internal microservice APIs for user identifier resolution - not intended for direct client use")]
public class UserResolutionController : ApiControllerBase
{
    private readonly ICachedUserResolutionService _userResolutionService;
    private readonly ILogger<UserResolutionController> _logger;

    public UserResolutionController(ICachedUserResolutionService userResolutionService, ILogger<UserResolutionController> logger)
    {
        _userResolutionService = userResolutionService;
        _logger = logger;
    }

    /// <summary>
    /// Resolves multiple usernames to their corresponding user IDs in batch
    /// </summary>
    /// <param name="request">Collection of usernames to resolve</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>Dictionary mapping usernames to user IDs for found users</returns>
    /// <response code="200">Usernames resolved successfully</response>
    /// <response code="400">Invalid request data or empty username list</response>
    /// <response code="500">Internal server error during resolution</response>
    /// <remarks>
    /// This is an internal API used by other microservices (e.g., Notes service) to resolve usernames to user IDs
    /// for operations like note sharing. Results are cached for performance optimization.
    /// Only existing users are returned in the response dictionary.
    /// </remarks>
    [HttpPost("resolve/batch/usernames")]
    [SwaggerOperation(
        Summary = "Batch resolve usernames to user IDs",
        Description = "Internal API endpoint that resolves a batch of usernames to their corresponding user IDs. Used by other microservices for user identification. Results include only existing users and are cached for performance.",
        OperationId = "BatchResolveUsernames",
        Tags = new[] { "UserResolution" }
    )]
    [ProducesResponseType<Dictionary<string, Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserIdsBatch(
        [FromBody, SwaggerRequestBody("Collection of usernames to resolve to user IDs")] BatchUserResolutionRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _userResolutionService.GetUserIdsBatchAsync(request.Usernames, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Resolves multiple user IDs to their corresponding usernames in batch
    /// </summary>
    /// <param name="request">Collection of user IDs to resolve</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>Dictionary mapping user IDs to usernames for found users</returns>
    /// <response code="200">User IDs resolved successfully</response>
    /// <response code="400">Invalid request data or empty user ID list</response>
    /// <response code="500">Internal server error during resolution</response>
    /// <remarks>
    /// This is an internal API used by other microservices (e.g., Notes service) to resolve user IDs to usernames
    /// for display purposes like showing note owners and shared users. Results are cached for performance optimization.
    /// Only existing users are returned in the response dictionary.
    /// </remarks>
    [HttpPost("resolve/batch/userids")]
    [SwaggerOperation(
        Summary = "Batch resolve user IDs to usernames",
        Description = "Internal API endpoint that resolves a batch of user IDs to their corresponding usernames. Used by other microservices for displaying user-friendly names. Results include only existing users and are cached for performance.",
        OperationId = "BatchResolveUserIds",
        Tags = new[] { "UserResolution" }
    )]
    [ProducesResponseType<Dictionary<Guid, string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsernamesBatch(
        [FromBody, SwaggerRequestBody("Collection of user IDs to resolve to usernames")] BatchUserIdResolutionRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _userResolutionService.GetUsernamesBatchAsync(request.UserIds, cancellationToken);
        return result.ToActionResult();
    }
}