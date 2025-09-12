using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
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

    public async Task<IEnumerable<Note>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Include(n => n.NoteTags)
            .AsNoTracking()
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

    public async Task<IEnumerable<Note>> SearchNotesAsync(string? searchText, List<string>? tags, CancellationToken ct = default)
    {
        var q = _context.Notes.AsNoTracking().AsQueryable();

        if (tags is { Count: > 0 })
        {
            var normalized = tags.Select(t => t.ToLowerInvariant()).ToList();
            q = q.Where(n => n.NoteTags.Any(nt => normalized.Contains(nt.TagName)));
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var trimmedSearchText = searchText.Trim();
            
            q = q.Where(n => EF.Property<NpgsqlTsVector>(n, "SearchVector").Matches(EF.Functions.WebSearchToTsQuery("italian", trimmedSearchText)))
                 .OrderByDescending(n => EF.Property<NpgsqlTsVector>(n, "SearchVector").RankCoverDensity(EF.Functions.WebSearchToTsQuery("italian", trimmedSearchText)))
                 .ThenByDescending(n => n.UpdatedAt);
        }
        else
        {
            q = q.OrderByDescending(n => n.UpdatedAt);
        }

        q = q.Include(n => n.NoteTags);

        return await q.ToListAsync(ct);
    }
}