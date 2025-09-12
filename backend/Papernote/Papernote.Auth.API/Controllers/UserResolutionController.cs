using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Papernote.Auth.Core.Application.DTOs;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.Auth.API.Extensions;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.API.Controllers;

[Route("api/internal/users")]
public class UserResolutionController : ApiControllerBase
{
    private readonly ICachedUserResolutionService _userResolutionService;
    private readonly ILogger<UserResolutionController> _logger;

    public UserResolutionController(ICachedUserResolutionService userResolutionService, ILogger<UserResolutionController> logger)
    {
        _userResolutionService = userResolutionService;
        _logger = logger;
    }

    [HttpPost("resolve/batch/usernames")]
    [ProducesResponseType<Dictionary<string, Guid>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserIdsBatch([FromBody] BatchUserResolutionRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _userResolutionService.GetUserIdsBatchAsync(request.Usernames, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("resolve/batch/userids")]
    [ProducesResponseType<Dictionary<Guid, string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsernamesBatch([FromBody] BatchUserIdResolutionRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _userResolutionService.GetUsernamesBatchAsync(request.UserIds, cancellationToken);
        return result.ToActionResult();
    }
}