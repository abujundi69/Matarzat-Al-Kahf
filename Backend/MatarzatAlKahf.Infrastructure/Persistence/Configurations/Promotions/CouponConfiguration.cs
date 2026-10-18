using MatarzatAlKahf.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Promotions;

public sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons", "promotions");
        builder.ConfigureBaseEntity();

        builder.Property(coupon => coupon.Code)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(coupon => coupon.DiscountType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(coupon => coupon.DiscountValue).HasPrecision(18, 2);
        builder.Property(coupon => coupon.MinimumOrderAmount).HasPrecision(18, 2);
        builder.Property(coupon => coupon.MaximumDiscountAmount).HasPrecision(18, 2);

        builder.Property(coupon => coupon.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.HasIndex(coupon => coupon.Code)
            .IsUnique();

        builder.HasIndex(coupon => new { coupon.Status, coupon.StartsAtUtc, coupon.EndsAtUtc });
    }
}
