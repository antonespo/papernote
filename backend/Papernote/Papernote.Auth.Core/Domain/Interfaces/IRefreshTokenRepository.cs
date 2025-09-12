using Papernote.Auth.Core.Domain.Entities;

namespace Papernote.Auth.Core.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}