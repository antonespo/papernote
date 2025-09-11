using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for NoteTag junction table
/// </summary>
public class NoteTagConfiguration : IEntityTypeConfiguration<NoteTag>
{
    public void Configure(EntityTypeBuilder<NoteTag> builder)
    {
        // Table configuration
        builder.ToTable("note_tags");

        // Composite Primary Key
        builder.HasKey(nt => new { nt.NoteId, nt.TagName });

        // Foreign Keys
        builder.Property(nt => nt.NoteId)
            .HasColumnName("note_id")
            .IsRequired();

        builder.Property(nt => nt.TagName)
            .HasColumnName("tag_name")
            .HasMaxLength(50)
            .IsRequired();

        // AddedAt timestamp
        builder.Property(nt => nt.AddedAt)
            .HasColumnName("added_at")
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(nt => nt.NoteId)
            .HasDatabaseName("ix_note_tags_note_id");

        builder.HasIndex(nt => nt.TagName)
            .HasDatabaseName("ix_note_tags_tag_name");

        builder.HasIndex(nt => nt.AddedAt)
            .HasDatabaseName("ix_note_tags_added_at");
    }
}