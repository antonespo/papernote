using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Papernote.Auth.API.Extensions;
using Papernote.Auth.Core.Application.DTOs;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.Auth.Infrastructure.Attributes;
using Papernote.SharedMicroservices.Results;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;

namespace Papernote.Auth.API.Controllers;

/// <summary>
/// Handles user authentication operations including registration, login, token refresh and logout
/// </summary>
[Route("api/v1/auth")]
[ApiController]
[SwaggerTag("Authentication operations for user registration, login, token management and logout")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRateLimitService _rateLimitService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IRateLimitService rateLimitService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _rateLimitService = rateLimitService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">User registration data including username and password</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>Authentication response with JWT tokens upon successful registration</returns>
    /// <response code="200">User registered successfully with JWT tokens</response>
    /// <response code="400">Invalid registration data provided</response>
    /// <response code="409">Username already exists</response>
    /// <response code="422">Validation errors in registration data</response>
    /// <response code="500">Internal server error during registration</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Register a new user account",
        Description = "Creates a new user account with the provided username and password. Passwords are securely hashed using Argon2id. Upon successful registration, returns JWT access and refresh tokens.",
        OperationId = "RegisterUser",
        Tags = new[] { "Auth" }
    )]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(
        [FromBody, SwaggerRequestBody("User registration data with username and password")] RegisterDto request,
        CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _authService.RegisterAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens
    /// </summary>
    /// <param name="request">User login credentials</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>Authentication response with JWT tokens upon successful login</returns>
    /// <response code="200">Login successful with JWT tokens</response>
    /// <response code="400">Invalid login data provided</response>
    /// <response code="401">Invalid username or password</response>
    /// <response code="403">Account locked or disabled</response>
    /// <response code="429">Too many login attempts - rate limit exceeded</response>
    /// <response code="500">Internal server error during login</response>
    [HttpPost("login")]
    [RateLimit("login")]
    [SwaggerOperation(
        Summary = "Authenticate user login",
        Description = "Authenticates a user with username and password. Implements rate limiting to prevent brute force attacks. Returns JWT access and refresh tokens upon successful authentication.",
        OperationId = "LoginUser",
        Tags = new[] { "Auth" }
    )]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(
        [FromBody, SwaggerRequestBody("User login credentials")] LoginDto request,
        CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _authService.LoginAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var clearResult = await _rateLimitService.ClearAttemptsAsync(request.Username, "login", cancellationToken);
            if (!clearResult.IsSuccess)
            {
                _logger.LogWarning("Failed to clear rate limit for successful login: {Error}", clearResult.Error);
            }
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Refreshes an expired JWT access token using a valid refresh token
    /// </summary>
    /// <param name="request">Refresh token data</param>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>New JWT tokens upon successful refresh</returns>
    /// <response code="200">Token refreshed successfully with new JWT tokens</response>
    /// <response code="400">Invalid refresh token data</response>
    /// <response code="401">Refresh token is invalid or expired</response>
    /// <response code="403">Refresh token has been revoked or user account disabled</response>
    /// <response code="500">Internal server error during token refresh</response>
    [HttpPost("refresh")]
    [SwaggerOperation(
        Summary = "Refresh JWT access token",
        Description = "Exchanges a valid refresh token for new JWT access and refresh tokens. Implements refresh token rotation for enhanced security. The provided refresh token is invalidated and a new one is issued.",
        OperationId = "RefreshToken",
        Tags = new[] { "Auth" }
    )]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken(
        [FromBody, SwaggerRequestBody("Refresh token for obtaining new JWT tokens")] RefreshTokenDto request,
        CancellationToken cancellationToken)
    {
        var validationError = this.ValidateModelState();
        if (validationError != null)
            return validationError.ToActionResult();

        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Logs out the authenticated user and invalidates their tokens
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the request</param>
    /// <returns>No content upon successful logout</returns>
    /// <response code="204">Logout successful - tokens invalidated</response>
    /// <response code="401">User is not authenticated or token is invalid</response>
    /// <response code="500">Internal server error during logout</response>
    [HttpPost("logout")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Logout authenticated user",
        Description = "Logs out the currently authenticated user by invalidating their access token and all associated refresh tokens. The access token is added to a blacklist to prevent further use.",
        OperationId = "LogoutUser",
        Tags = new[] { "Auth" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ResultBuilder.BadRequest("Invalid user ID in token").ToActionResult();
        }

        var accessToken = HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return ResultBuilder.BadRequest("Access token is required").ToActionResult();
        }

        var result = await _authService.LogoutAsync(userId, accessToken, cancellationToken);
        return result.ToActionResult();
    }
}