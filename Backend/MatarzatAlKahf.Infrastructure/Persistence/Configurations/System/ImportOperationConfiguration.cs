using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.System;

public sealed class ImportOperationConfiguration :
    IEntityTypeConfiguration<ImportOperation>
{
    public void Configure(EntityTypeBuilder<ImportOperation> builder)
    {
        builder.ToTable("ImportOperations", "system");
        builder.ConfigureBaseEntity();
        builder.Property(item => item.Type).HasConversion<string>().HasMaxLength(20).IsUnicode();
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(20).IsUnicode();
        builder.Property(item => item.OriginalFileName)
            .IsRequired().HasMaxLength(255).IsUnicode();
        builder.Property(item => item.FileHash).IsRequired().HasMaxLength(64).IsUnicode();
        builder.Property(item => item.RowsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(item => item.ErrorsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(item => item.CommitIdempotencyKey)
            .IsRequired().HasMaxLength(200).IsUnicode();
        builder.HasOne(item => item.AdminUser).WithMany()
            .HasForeignKey(item => item.AdminUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.AdminUserId, item.Type, item.CreatedAtUtc });
        builder.HasIndex(item => item.FileHash);
        builder.HasIndex(item => item.ExpiresAtUtc);
        builder.HasIndex(item => item.CommitIdempotencyKey)
            .IsUnique()
            .HasFilter("[CommitIdempotencyKey] <> N''");
    }
}
