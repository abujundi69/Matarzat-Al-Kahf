using MatarzatAlKahf.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Orders;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", "sales");
        builder.ConfigureBaseEntity();

        builder.Property(item => item.ProductNameSnapshot)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(item => item.SkuSnapshot)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(item => item.VariantDescriptionSnapshot)
            .IsRequired()
            .HasMaxLength(1000)
            .IsUnicode();

        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.TotalPrice).HasPrecision(18, 2);

        builder.HasOne(item => item.Order)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Product)
            .WithMany(product => product.OrderItems)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(item => item.ProductVariant)
            .WithMany(variant => variant.OrderItems)
            .HasForeignKey(item => item.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => item.ProductId);
        builder.HasIndex(item => item.ProductVariantId);
    }
}
