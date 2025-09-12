using Papernote.Auth.Core.Application.DTOs;
using Papernote.SharedMicroservices.Results;

namespace Papernote.Auth.Core.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> RevokeAllTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}