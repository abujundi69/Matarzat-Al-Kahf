using MatarzatAlKahf.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Orders;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems", "sales");
        builder.ConfigureBaseEntity();
        builder.HasOne(item => item.Cart).WithMany(cart => cart.Items)
            .HasForeignKey(item => item.CartId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.ProductVariant).WithMany()
            .HasForeignKey(item => item.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.CartId, item.ProductVariantId }).IsUnique();
        builder.HasIndex(item => item.ProductVariantId);
    }
}
