using MatarzatAlKahf.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages", "catalog");
        builder.ConfigureBaseEntity();

        builder.Property(image => image.ImageUrl)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.Property(image => image.ThumbnailUrl)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.Property(image => image.SortOrder)
            .HasDefaultValue(0);

        builder.Property(image => image.IsPrimary)
            .HasDefaultValue(false);

        builder.HasOne(image => image.Product)
            .WithMany(product => product.Images)
            .HasForeignKey(image => image.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(image => new { image.ProductId, image.SortOrder });
        builder.HasIndex(image => new { image.ProductId, image.IsPrimary });
    }
}
