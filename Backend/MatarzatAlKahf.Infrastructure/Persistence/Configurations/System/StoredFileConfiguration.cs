using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.System;

public sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("StoredFiles", "system");
        builder.ConfigureBaseEntity();
        builder.Property(file => file.StorageKey).IsRequired().HasMaxLength(500).IsUnicode();
        builder.Property(file => file.OriginalFileName).IsRequired().HasMaxLength(255).IsUnicode();
        builder.Property(file => file.ContentType).IsRequired().HasMaxLength(100).IsUnicode();
        builder.Property(file => file.Sha256).IsRequired().HasMaxLength(64).IsUnicode();
        builder.HasIndex(file => file.StorageKey).IsUnique();
        builder.HasIndex(file => file.Sha256);
        builder.HasIndex(file => file.UploadedByAdminUserId);
        builder.HasIndex(file => file.UploadedByCustomerId);
    }
}
