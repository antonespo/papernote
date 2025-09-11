using Microsoft.EntityFrameworkCore;
using Papernote.Notes.Core.Domain.Entities;
using Papernote.Notes.Core.Domain.Interfaces;
using Papernote.Notes.Infrastructure.Persistence;

namespace Papernote.Notes.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Notes using Entity Framework and PostgreSQL
/// </summary>
public class NoteRepository : INoteRepository
{
    private readonly NotesDbContext _context;

    public NoteRepository(NotesDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Note?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
            .Include(n => n.NoteShares)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetByUserIdAsync(Guid userId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Notes
            .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
            .Include(n => n.NoteShares)
            .AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetByTagAsync(string tag, Guid userId, CancellationToken cancellationToken = default)
    {
        var tagName = tag.ToLowerInvariant();

        return await _context.Notes
            .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
            .Include(n => n.NoteShares)
            .Where(n => n.UserId == userId)
            .Where(n => n.NoteTags.Any(nt => nt.Tag.Name == tagName))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);
        return note;
    }

    public async Task<Note> UpdateAsync(Note note, CancellationToken cancellationToken = default)
    {
        _context.Notes.Update(note);
        await _context.SaveChangesAsync(cancellationToken);
        return note;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var note = await GetByIdAsync(id, cancellationToken);
        if (note != null)
        {
            note.MarkAsDeleted();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .AnyAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .CountAsync(n => n.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Basic text search in user's notes using LIKE operator
    /// </summary>
    public async Task<IEnumerable<Note>> SearchAsync(string searchText, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return await GetByUserIdAsync(userId, false, cancellationToken);
        }

        var searchTerm = $"%{searchText.ToLowerInvariant()}%";

        return await _context.Notes
            .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
            .Include(n => n.NoteShares)
            .Where(n => n.UserId == userId)
            .Where(n => EF.Functions.Like(n.Title.ToLower(), searchTerm) || 
                       EF.Functions.Like(n.Content.ToLower(), searchTerm))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get notes shared with a specific user
    /// </summary>
    public async Task<IEnumerable<Note>> GetSharedWithUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
            .Include(n => n.NoteShares)
            .Where(n => n.NoteShares.Any(ns => ns.SharedWithUserId == userId && !ns.IsRevoked))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get notes shared by a specific user
    /// </summary>
    public async Task<IEnumerable<Note>> GetSharedByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
            .Include(n => n.NoteShares)
            .Where(n => n.UserId == userId && n.NoteShares.Any(ns => !ns.IsRevoked))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}