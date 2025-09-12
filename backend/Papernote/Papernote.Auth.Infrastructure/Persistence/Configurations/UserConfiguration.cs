using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Papernote.Auth.Core.Domain.Entities;

namespace Papernote.Auth.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at")
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("ix_users_username_unique");

        builder.HasIndex(u => u.Status)
            .HasDatabaseName("ix_users_status");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("ix_users_created_at");

        builder.HasIndex(u => u.LastLoginAt)
            .HasDatabaseName("ix_users_last_login_at");
    }
}