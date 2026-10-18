using MatarzatAlKahf.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductImageTranslationConfiguration : IEntityTypeConfiguration<ProductImageTranslation>
{
    public void Configure(EntityTypeBuilder<ProductImageTranslation> builder)
    {
        builder.ToTable("ProductImageTranslations", "catalog");
        builder.ConfigureBaseEntity();

        builder.Property(translation => translation.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(translation => translation.AltText)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode();

        builder.HasOne(translation => translation.ProductImage)
            .WithMany(image => image.Translations)
            .HasForeignKey(translation => translation.ProductImageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(translation => new { translation.ProductImageId, translation.Language })
            .IsUnique();
    }
}
