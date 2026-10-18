using MatarzatAlKahf.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Promotions;

public sealed class CouponProductConfiguration : IEntityTypeConfiguration<CouponProduct>
{
    public void Configure(EntityTypeBuilder<CouponProduct> builder)
    {
        builder.ToTable("CouponProducts", "promotions");
        builder.HasKey(mapping => new { mapping.CouponId, mapping.ProductId });
        builder.HasOne(mapping => mapping.Coupon).WithMany(coupon => coupon.Products)
            .HasForeignKey(mapping => mapping.CouponId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(mapping => mapping.Product).WithMany()
            .HasForeignKey(mapping => mapping.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(mapping => mapping.ProductId);
    }
}
