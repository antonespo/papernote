using Papernote.Auth.Core.Domain.Entities;

namespace Papernote.Auth.Core.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<Dictionary<string, Guid>> GetUserIdsByUsernamesAsync(IEnumerable<string> usernames, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, string>> GetUsernamesByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}