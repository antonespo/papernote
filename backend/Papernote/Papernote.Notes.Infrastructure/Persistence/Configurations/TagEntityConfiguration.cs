using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for TagEntity
/// </summary>
public class TagEntityConfiguration : IEntityTypeConfiguration<TagEntity>
{
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        // Table configuration
        builder.ToTable("tags");

        // Primary Key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        // Name
        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(32)
            .IsRequired();

        // CreatedAt
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationships
        builder.HasMany(t => t.NoteTags)
            .WithOne(nt => nt.Tag)
            .HasForeignKey(nt => nt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint on name
        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("ix_tags_name_unique");

        // Index for performance
        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_tags_created_at");
    }
}