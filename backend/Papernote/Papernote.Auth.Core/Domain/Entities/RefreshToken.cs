namespace Papernote.Auth.Core.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenId { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public RefreshToken(Guid userId, string tokenHash, TimeSpan validFor)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash ?? throw new ArgumentNullException(nameof(tokenHash));
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.Add(validFor);
        IsRevoked = false;
    }

    public void Revoke(string? replacedByTokenId = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }

    public bool IsValid => !IsRevoked && DateTime.UtcNow < ExpiresAt;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}