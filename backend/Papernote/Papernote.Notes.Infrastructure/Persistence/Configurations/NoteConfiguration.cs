using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Infrastructure.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("notes");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Content)
            .HasColumnName("content")
            .HasMaxLength(50000)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(n => n.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property<NpgsqlTsVector>("SearchVector")
            .HasColumnName("search_vector")
            .HasColumnType("tsvector")
            .HasComputedColumnSql(@"
                setweight(to_tsvector('italian', coalesce(title,   '')), 'A') ||
                setweight(to_tsvector('italian', coalesce(content, '')), 'B')
            ", stored: true)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasMany(n => n.NoteTags)
            .WithOne(nt => nt.Note)
            .HasForeignKey(nt => nt.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.CreatedAt)
            .HasDatabaseName("ix_notes_created_at");

        builder.HasIndex(n => n.UpdatedAt)
            .HasDatabaseName("ix_notes_updated_at");

        builder.HasIndex(n => n.IsDeleted)
            .HasDatabaseName("ix_notes_is_deleted");

        builder.HasIndex("SearchVector")
            .HasDatabaseName("ix_notes_search_vector")
            .HasMethod("gin");

        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}