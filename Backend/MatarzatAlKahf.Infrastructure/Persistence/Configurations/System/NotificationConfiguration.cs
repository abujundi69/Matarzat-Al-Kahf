using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.System;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications", "system");
        builder.ConfigureBaseEntity();

        builder.Property(notification => notification.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(notification => notification.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(notification => notification.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(notification => notification.Title)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(notification => notification.Body)
            .IsRequired()
            .HasMaxLength(4000)
            .IsUnicode();

        builder.Property(notification => notification.Recipient)
            .IsRequired()
            .HasMaxLength(320)
            .IsUnicode();

        builder.Property(notification => notification.ProviderReference)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(notification => notification.FailureReason)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.Property(notification => notification.RetryCount)
            .HasDefaultValue(0);

        builder.HasOne(notification => notification.Customer)
            .WithMany(customer => customer.Notifications)
            .HasForeignKey(notification => notification.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(notification => notification.AdminUser)
            .WithMany(adminUser => adminUser.Notifications)
            .HasForeignKey(notification => notification.AdminUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(notification => notification.CustomerId);
        builder.HasIndex(notification => notification.AdminUserId);
        builder.HasIndex(notification => new
        {
            notification.Status,
            notification.Channel,
            notification.CreatedAtUtc,
        });
        builder.HasIndex(notification => notification.ProviderReference);
        builder.HasIndex(notification => new
        {
            notification.Status,
            notification.NextAttemptAtUtc,
        });
        builder.HasIndex(notification => notification.OrderId);
    }
}
