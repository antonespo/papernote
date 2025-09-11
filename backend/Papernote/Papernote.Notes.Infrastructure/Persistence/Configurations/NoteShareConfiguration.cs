using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for NoteShare entity
/// </summary>
public class NoteShareConfiguration : IEntityTypeConfiguration<NoteShare>
{
    public void Configure(EntityTypeBuilder<NoteShare> builder)
    {
        // Table configuration
        builder.ToTable("note_shares");

        // Primary Key
        builder.HasKey(ns => ns.Id);
        builder.Property(ns => ns.Id)
            .HasColumnName("id")
            .IsRequired();

        // Foreign Keys
        builder.Property(ns => ns.NoteId)
            .HasColumnName("note_id")
            .IsRequired();

        builder.Property(ns => ns.SharedWithUserId)
            .HasColumnName("shared_with_user_id")
            .IsRequired();

        builder.Property(ns => ns.SharedByUserId)
            .HasColumnName("shared_by_user_id")
            .IsRequired();

        // Timestamps
        builder.Property(ns => ns.SharedAt)
            .HasColumnName("shared_at")
            .IsRequired();

        // IsRevoked flag
        builder.Property(ns => ns.IsRevoked)
            .HasColumnName("is_revoked")
            .HasDefaultValue(false);

        // Relationships are configured in Note configuration

        // Unique constraint: one active share per note-user combination
        builder.HasIndex(ns => new { ns.NoteId, ns.SharedWithUserId })
            .IsUnique()
            .HasFilter("is_revoked = false")
            .HasDatabaseName("ix_note_shares_note_user_active");

        // Indexes for performance
        builder.HasIndex(ns => ns.NoteId)
            .HasDatabaseName("ix_note_shares_note_id");

        builder.HasIndex(ns => ns.SharedWithUserId)
            .HasDatabaseName("ix_note_shares_shared_with_user_id");

        builder.HasIndex(ns => ns.SharedByUserId)
            .HasDatabaseName("ix_note_shares_shared_by_user_id");

        builder.HasIndex(ns => ns.SharedAt)
            .HasDatabaseName("ix_note_shares_shared_at");

        builder.HasIndex(ns => ns.IsRevoked)
            .HasDatabaseName("ix_note_shares_is_revoked");
    }
}