using MatarzatAlKahf.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants", "catalog");
        builder.ConfigureBaseEntity();

        builder.Property(variant => variant.Sku)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(variant => variant.Size)
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(variant => variant.Color)
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(variant => variant.WeightInGrams)
            .HasPrecision(18, 3);

        builder.Property(variant => variant.Capacity)
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(variant => variant.PriceOverride)
            .HasPrecision(18, 2);

        builder.Property(variant => variant.IsActive)
            .HasDefaultValue(false);

        builder.HasOne(variant => variant.Product)
            .WithMany(product => product.Variants)
            .HasForeignKey(variant => variant.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(variant => variant.Sku)
            .IsUnique();

        builder.HasIndex(variant => new { variant.ProductId, variant.IsActive });
    }
}
