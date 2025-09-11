using Microsoft.EntityFrameworkCore;
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
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        var tagName = tag.ToLowerInvariant();

        return await _context.Notes
            .Include(n => n.NoteTags)
            .Where(n => n.NoteTags.Any(nt => nt.TagName == tagName))
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

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return await GetAllAsync(cancellationToken);
        }

        var searchTerm = $"%{searchText.ToLowerInvariant()}%";

        return await _context.Notes
            .Include(n => n.NoteTags)
            .Where(n => EF.Functions.Like(n.Title.ToLower(), searchTerm) ||
                       EF.Functions.Like(n.Content.ToLower(), searchTerm))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}