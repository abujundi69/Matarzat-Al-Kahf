using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.System;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", "system");
        builder.ConfigureBaseEntity();

        builder.Property(log => log.Action)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(log => log.ActorName)
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(log => log.ActorType)
            .IsRequired()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(log => log.EntityName)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(log => log.Details)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(log => log.IpAddress)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(log => log.UserAgent)
            .IsRequired()
            .HasMaxLength(1000)
            .IsUnicode();

        builder.Property(log => log.TraceId)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.HasOne(log => log.AdminUser)
            .WithMany(adminUser => adminUser.AuditLogs)
            .HasForeignKey(log => log.AdminUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(log => log.AdminUserId);
        builder.HasIndex(log => new { log.EntityName, log.EntityId });
        builder.HasIndex(log => new { log.Action, log.CreatedAtUtc });
    }
}
