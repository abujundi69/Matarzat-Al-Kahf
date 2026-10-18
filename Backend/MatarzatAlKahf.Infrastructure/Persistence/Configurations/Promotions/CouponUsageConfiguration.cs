using MatarzatAlKahf.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Promotions;

public sealed class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("CouponUsages", "promotions");
        builder.ConfigureBaseEntity();

        builder.Property(usage => usage.DiscountAmount).HasPrecision(18, 2);

        builder.HasOne(usage => usage.Coupon)
            .WithMany(coupon => coupon.Usages)
            .HasForeignKey(usage => usage.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(usage => usage.Order)
            .WithOne(order => order.CouponUsage)
            .HasForeignKey<CouponUsage>(usage => usage.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(usage => usage.Customer)
            .WithMany(customer => customer.CouponUsages)
            .HasForeignKey(usage => usage.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(usage => usage.OrderId)
            .IsUnique();

        builder.HasIndex(usage => usage.CouponId);
        builder.HasIndex(usage => usage.CustomerId);
    }
}
