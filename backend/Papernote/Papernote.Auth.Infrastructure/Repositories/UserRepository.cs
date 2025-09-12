using Microsoft.EntityFrameworkCore;
using Papernote.Auth.Core.Domain.Entities;
using Papernote.Auth.Core.Domain.Interfaces;
using Papernote.Auth.Infrastructure.Persistence;

namespace Papernote.Auth.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant(), cancellationToken);
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Username == username.ToLowerInvariant(), cancellationToken);
    }

    public async Task<Dictionary<string, Guid>> GetUserIdsByUsernamesAsync(IEnumerable<string> usernames, CancellationToken cancellationToken = default)
    {
        var normalizedUsernames = usernames.Select(u => u.ToLowerInvariant()).ToList();
        
        return await _context.Users
            .AsNoTracking()
            .Where(u => normalizedUsernames.Contains(u.Username))
            .ToDictionaryAsync(u => u.Username, u => u.Id, cancellationToken);
    }

    public async Task<Dictionary<Guid, string>> GetUsernamesByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var userIdsList = userIds.ToList();
        
        return await _context.Users
            .AsNoTracking()
            .Where(u => userIdsList.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username, cancellationToken);
    }
}