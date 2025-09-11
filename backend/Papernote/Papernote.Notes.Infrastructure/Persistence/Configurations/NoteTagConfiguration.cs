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
        builder.HasKey(nt => new { nt.NoteId, nt.TagId });

        // Foreign Keys
        builder.Property(nt => nt.NoteId)
            .HasColumnName("note_id")
            .IsRequired();

        builder.Property(nt => nt.TagId)
            .HasColumnName("tag_id")
            .IsRequired();

        // AddedAt timestamp
        builder.Property(nt => nt.AddedAt)
            .HasColumnName("added_at")
            .IsRequired();

        // Relationships are configured in Note and TagEntity configurations

        // Indexes for performance
        builder.HasIndex(nt => nt.NoteId)
            .HasDatabaseName("ix_note_tags_note_id");

        builder.HasIndex(nt => nt.TagId)
            .HasDatabaseName("ix_note_tags_tag_id");

        builder.HasIndex(nt => nt.AddedAt)
            .HasDatabaseName("ix_note_tags_added_at");
    }
}