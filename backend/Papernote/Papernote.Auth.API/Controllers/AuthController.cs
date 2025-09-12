using Microsoft.AspNetCore.Mvc;
using Papernote.Auth.Core.Application.DTOs;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.Auth.API.Extensions;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.API.Controllers;

[Route("api/v1/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterDto request, CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _authService.RegisterAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _authService.LoginAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request, CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto request, CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _authService.LogoutAsync(request.UserId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("revoke-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeAllTokens([FromBody] RevokeAllTokensDto request, CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _authService.RevokeAllTokensAsync(request.UserId, cancellationToken);
        return result.ToActionResult();
    }
}