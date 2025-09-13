using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Domain.Entities;
using Papernote.Notes.Core.Domain.Interfaces;
using Papernote.Notes.Infrastructure.Persistence;

namespace Papernote.Notes.Infrastructure.Repositories;

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
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<Note?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Include(n => n.NoteTags)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<Note?> GetByIdWithTagsAndSharesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Include(n => n.NoteTags)
            .Include(n => n.NoteShares)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Include(n => n.NoteTags)
            .Include(n => n.NoteShares)
            .AsNoTracking()
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetNotesAsync(GetNotesDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        var query = _context.Notes.AsNoTracking();

        query = request.Filter switch
        {
            NoteFilter.Owned => query.Where(n => n.OwnerUserId == userId),
            NoteFilter.Shared => query.Where(n => n.NoteShares.Any(ns => ns.ReaderUserId == userId)),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Filter), "Invalid filter value")
        };

        if (request.IsSearch)
        {
            if (request.SearchTags is { Count: > 0 })
            {
                var normalizedTags = request.SearchTags.Select(t => t.ToLowerInvariant()).ToList();
                query = query.Where(n => n.NoteTags.Any(nt => normalizedTags.Contains(nt.TagName)));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var trimmedSearchText = request.SearchText.Trim();

                query = query
                    .Where(n => EF.Property<NpgsqlTsVector>(n, "SearchVector")
                        .Matches(EF.Functions.WebSearchToTsQuery("italian", trimmedSearchText)))
                    .OrderByDescending(n => EF.Property<NpgsqlTsVector>(n, "SearchVector")
                        .RankCoverDensity(EF.Functions.WebSearchToTsQuery("italian", trimmedSearchText)))
                    .ThenByDescending(n => n.UpdatedAt);
            }
            else
            {
                query = query.OrderByDescending(n => n.UpdatedAt);
            }
        }
        else
        {
            query = query.OrderByDescending(n => n.UpdatedAt);
        }

        return await query
            .Include(n => n.NoteTags)
            .Include(n => n.NoteShares)
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

    public async Task<bool> CanUserReadNoteAsync(Guid noteId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .AnyAsync(n => n.Id == noteId &&
                          (n.OwnerUserId == userId || n.NoteShares.Any(ns => ns.ReaderUserId == userId)),
                      cancellationToken);
    }

    public async Task<bool> CanUserWriteNoteAsync(Guid noteId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .AnyAsync(n => n.Id == noteId && n.OwnerUserId == userId, cancellationToken);
    }
}