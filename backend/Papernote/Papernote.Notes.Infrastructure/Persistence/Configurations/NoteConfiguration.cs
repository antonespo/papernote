using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Note entity
/// </summary>
public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        // Table mapping
        builder.ToTable("notes");

        // Primary Key
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
            .HasColumnName("id")
            .IsRequired();

        // Title
        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        // Content
        builder.Property(n => n.Content)
            .HasColumnName("content")
            .HasMaxLength(50000)
            .IsRequired();

        // Timestamps
        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // IsDeleted (soft delete)
        builder.Property(n => n.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        // Relationships
        builder.HasMany(n => n.NoteTags)
            .WithOne(nt => nt.Note)
            .HasForeignKey(nt => nt.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(n => n.CreatedAt)
            .HasDatabaseName("ix_notes_created_at");

        builder.HasIndex(n => n.UpdatedAt)
            .HasDatabaseName("ix_notes_updated_at");

        builder.HasIndex(n => n.IsDeleted)
            .HasDatabaseName("ix_notes_is_deleted");

        // Query filter for soft delete
        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}