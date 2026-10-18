using MatarzatAlKahf.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "catalog");
        builder.ConfigureBaseEntity();

        builder.Property(product => product.BasePrice)
            .HasPrecision(18, 2);

        builder.Property(product => product.Currency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsUnicode();

        builder.Property(product => product.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(product => product.IsFeatured)
            .HasDefaultValue(false);

        builder.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(product => product.CategoryId);
        builder.HasIndex(product => new { product.Status, product.IsFeatured });
        builder.HasIndex(product => product.PublishedAtUtc);
    }
}
