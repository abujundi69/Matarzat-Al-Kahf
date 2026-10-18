using MatarzatAlKahf.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Orders;

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts", "sales");
        builder.ConfigureBaseEntity();
        builder.Property(cart => cart.GuestTokenHash).IsRequired().HasMaxLength(128).IsUnicode();
        builder.Property(cart => cart.Status).HasConversion<string>().HasMaxLength(20).IsUnicode();
        builder.Property(cart => cart.RowVersion).IsRowVersion();
        builder.HasOne(cart => cart.Customer).WithMany()
            .HasForeignKey(cart => cart.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(cart => cart.Coupon).WithMany()
            .HasForeignKey(cart => cart.CouponId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(cart => cart.GuestTokenHash)
            .IsUnique().HasFilter("[GuestTokenHash] <> N''");
        builder.HasIndex(cart => new { cart.CustomerId, cart.Status });
        builder.HasIndex(cart => cart.ExpiresAtUtc);
    }
}
