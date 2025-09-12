using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Papernote.Auth.Core.Application.Configuration;
using Papernote.Auth.Core.Application.DTOs;
using Papernote.Auth.Core.Application.Interfaces;
using Papernote.Auth.Core.Domain.Entities;
using Papernote.Auth.Core.Domain.Interfaces;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordValidator _passwordValidator;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly AuthSettings _authSettings;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IPasswordValidator passwordValidator,
        IJwtTokenService jwtTokenService,
        IMapper mapper,
        ILogger<AuthService> logger,
        IOptions<AuthSettings> authSettings)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _passwordValidator = passwordValidator;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
        _logger = logger;
        _authSettings = authSettings.Value;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        if (registerDto is null)
            return ResultBuilder.BadRequest<AuthResponseDto>("Registration data is required");

        try
        {
            // Validate password policy
            if (!_passwordValidator.IsValidPassword(registerDto.Password, _authSettings.PasswordSettings))
            {
                var errorMessage = _passwordValidator.GetValidationErrorMessage(registerDto.Password, _authSettings.PasswordSettings);
                return ResultBuilder.ValidationError<AuthResponseDto>(errorMessage);
            }

            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(registerDto.Username, cancellationToken))
            {
                _logger.LogWarning("Registration attempt with existing username: {Username}", registerDto.Username);
                return ResultBuilder.Conflict<AuthResponseDto>("Username already exists");
            }

            // Hash password
            var passwordHash = _passwordHasher.HashPassword(registerDto.Password);

            // Create user entity
            var user = new User(registerDto.Username, passwordHash);

            // Save to database
            var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

            // Generate tokens
            var authResponse = await GenerateAuthResponseAsync(createdUser, cancellationToken);

            _logger.LogInformation("User registered successfully: {UserId}", createdUser.Id);
            return ResultBuilder.Success(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for username: {Username}", registerDto.Username);
            return ResultBuilder.InternalServerError<AuthResponseDto>("Registration failed");
        }
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        if (loginDto is null)
            return ResultBuilder.BadRequest<AuthResponseDto>("Login data is required");

        try
        {
            // Find user by username
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent username: {Username}", loginDto.Username);
                return ResultBuilder.Unauthorized<AuthResponseDto>("Invalid credentials");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
                return ResultBuilder.Forbidden<AuthResponseDto>("Account is suspended or deleted");
            }

            // Verify password
            if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password attempt for user: {UserId}", user.Id);
                return ResultBuilder.Unauthorized<AuthResponseDto>("Invalid credentials");
            }

            // Update last login
            user.RecordLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Generate tokens
            var authResponse = await GenerateAuthResponseAsync(user, cancellationToken);

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);
            return ResultBuilder.Success(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", loginDto.Username);
            return ResultBuilder.InternalServerError<AuthResponseDto>("Login failed");
        }
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default)
    {
        if (refreshTokenDto is null)
            return ResultBuilder.BadRequest<AuthResponseDto>("Refresh token is required");

        try
        {
            var tokenHash = _jwtTokenService.HashRefreshToken(refreshTokenDto.RefreshToken);

            var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
            if (refreshToken == null || !refreshToken.IsValid)
            {
                _logger.LogWarning("Invalid or expired refresh token used");
                return ResultBuilder.Unauthorized<AuthResponseDto>("Invalid refresh token");
            }

            if (!refreshToken.User.IsActive)
            {
                _logger.LogWarning("Refresh token used for inactive user: {UserId}", refreshToken.User.Id);
                return ResultBuilder.Forbidden<AuthResponseDto>("Account is suspended or deleted");
            }

            var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
            var newRefreshTokenHash = _jwtTokenService.HashRefreshToken(newRefreshTokenValue);
            refreshToken.Revoke(newRefreshTokenHash);
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

            var newRefreshToken = new RefreshToken(
                refreshToken.User.Id,
                newRefreshTokenHash,
                _authSettings.JwtSettings.RefreshTokenLifetime);
            await _refreshTokenRepository.CreateAsync(newRefreshToken, cancellationToken);

            var accessToken = _jwtTokenService.GenerateAccessToken(refreshToken.User);
            var expiresAt = DateTime.UtcNow.Add(_authSettings.JwtSettings.AccessTokenLifetime);
            var userDto = _mapper.Map<UserDto>(refreshToken.User);

            var authResponse = new AuthResponseDto(
                accessToken,
                newRefreshTokenValue,
                expiresAt,
                userDto);

            _logger.LogInformation("Tokens refreshed successfully for user: {UserId}", refreshToken.User.Id);
            return ResultBuilder.Success(authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return ResultBuilder.InternalServerError<AuthResponseDto>("Token refresh failed");
        }
    }

    public async Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return ResultBuilder.BadRequest("Invalid user ID");

        try
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, cancellationToken);

            _logger.LogInformation("User logged out successfully: {UserId}", userId);
            return ResultBuilder.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return ResultBuilder.InternalServerError("Logout failed");
        }
    }

    public async Task<Result> RevokeAllTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return ResultBuilder.BadRequest("Invalid user ID");

        try
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, cancellationToken);

            _logger.LogInformation("All tokens revoked for user: {UserId}", userId);
            return ResultBuilder.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
            return ResultBuilder.InternalServerError("Token revocation failed");
        }
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var expiresAt = DateTime.UtcNow.Add(_authSettings.JwtSettings.AccessTokenLifetime);

        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = _jwtTokenService.HashRefreshToken(refreshTokenValue);

        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenHash,
            _authSettings.JwtSettings.RefreshTokenLifetime);
        await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);

        return new AuthResponseDto(
            accessToken,
            refreshTokenValue,
            expiresAt,
            userDto);
    }
}