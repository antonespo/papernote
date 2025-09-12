using Papernote.Auth.Core.Domain.Entities;

namespace Papernote.Auth.Core.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
    bool ValidateAccessToken(string token);
}