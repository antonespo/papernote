using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Infrastructure.Persistence.Configurations;

public class NoteShareConfiguration : IEntityTypeConfiguration<NoteShare>
{
    public void Configure(EntityTypeBuilder<NoteShare> builder)
    {
        builder.ToTable("note_shares");

        builder.HasKey(ns => new { ns.NoteId, ns.ReaderUserId });

        builder.Property(ns => ns.NoteId)
            .HasColumnName("note_id")
            .IsRequired();

        builder.Property(ns => ns.ReaderUserId)
            .HasColumnName("reader_user_id")
            .IsRequired();

        builder.Property(ns => ns.SharedAt)
            .HasColumnName("shared_at")
            .IsRequired();

        builder.HasOne(ns => ns.Note)
            .WithMany(n => n.NoteShares)
            .HasForeignKey(ns => ns.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ns => ns.NoteId)
            .HasDatabaseName("ix_note_shares_note_id");

        builder.HasIndex(ns => ns.ReaderUserId)
            .HasDatabaseName("ix_note_shares_reader_user_id");

        builder.HasIndex(ns => new { ns.NoteId, ns.ReaderUserId })
            .HasDatabaseName("ix_note_shares_note_reader")
            .IsUnique();

        builder.HasIndex(ns => ns.SharedAt)
            .HasDatabaseName("ix_note_shares_shared_at");
    }
}