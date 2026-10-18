using MatarzatAlKahf.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Promotions;

public sealed class CouponCategoryConfiguration : IEntityTypeConfiguration<CouponCategory>
{
    public void Configure(EntityTypeBuilder<CouponCategory> builder)
    {
        builder.ToTable("CouponCategories", "promotions");
        builder.HasKey(mapping => new { mapping.CouponId, mapping.CategoryId });
        builder.HasOne(mapping => mapping.Coupon).WithMany(coupon => coupon.Categories)
            .HasForeignKey(mapping => mapping.CouponId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(mapping => mapping.Category).WithMany()
            .HasForeignKey(mapping => mapping.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(mapping => mapping.CategoryId);
    }
}
