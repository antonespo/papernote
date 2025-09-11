using Microsoft.EntityFrameworkCore;
using Papernote.Notes.Core.Domain.Entities;
using Papernote.Notes.Infrastructure.Persistence.Configurations;

namespace Papernote.Notes.Infrastructure.Persistence;

/// <summary>
/// Database context for the Notes domain
/// </summary>
public class NotesDbContext : DbContext
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options) : base(options)
    {
    }

    public DbSet<Note> Notes { get; set; } = null!;
    public DbSet<TagEntity> Tags { get; set; } = null!;
    public DbSet<NoteTag> NoteTags { get; set; } = null!;
    public DbSet<NoteShare> NoteShares { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("papernote");

        modelBuilder.ApplyConfiguration(new NoteConfiguration());
        modelBuilder.ApplyConfiguration(new TagEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NoteTagConfiguration());
        modelBuilder.ApplyConfiguration(new NoteShareConfiguration());
    }
}