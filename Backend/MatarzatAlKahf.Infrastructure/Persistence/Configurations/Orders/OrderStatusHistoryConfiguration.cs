using MatarzatAlKahf.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Orders;

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory", "sales");
        builder.ConfigureBaseEntity();

        builder.Property(history => history.PreviousStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(history => history.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(history => history.Note)
            .HasMaxLength(2000)
            .IsUnicode();

        builder.HasOne(history => history.Order)
            .WithMany(order => order.StatusHistory)
            .HasForeignKey(history => history.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(history => history.AdminUser)
            .WithMany(adminUser => adminUser.OrderStatusChanges)
            .HasForeignKey(history => history.ChangedByAdminUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(history => new { history.OrderId, history.CreatedAtUtc });
        builder.HasIndex(history => history.ChangedByAdminUserId);
    }
}
