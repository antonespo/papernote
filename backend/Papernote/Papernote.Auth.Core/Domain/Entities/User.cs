namespace Papernote.Auth.Core.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User() { }

    public User(string username, string passwordHash)
    {
        Id = Guid.NewGuid();
        Username = ValidateAndNormalizeUsername(username);
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        Status = UserStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive => Status == UserStatus.Active;

    private static string ValidateAndNormalizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));
        
        if (username.Length < 3 || username.Length > 32)
            throw new ArgumentException("Username must be between 3 and 32 characters", nameof(username));
        
        if (!IsValidUsernameFormat(username))
            throw new ArgumentException("Username can only contain letters, numbers, and underscores", nameof(username));

        return username.ToLowerInvariant();
    }

    private static bool IsValidUsernameFormat(string username)
    {
        return username.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}