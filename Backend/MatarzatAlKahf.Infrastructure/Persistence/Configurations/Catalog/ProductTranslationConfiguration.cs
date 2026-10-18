using MatarzatAlKahf.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Catalog;

public sealed class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("ProductTranslations", "catalog");
        builder.ConfigureBaseEntity();

        builder.Property(translation => translation.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(translation => translation.Name)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(translation => translation.Slug)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(translation => translation.ShortDescription)
            .IsRequired()
            .HasMaxLength(1000)
            .IsUnicode();

        builder.Property(translation => translation.Description)
            .IsRequired()
            .HasMaxLength(4000)
            .IsUnicode();

        builder.Property(translation => translation.Story)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(translation => translation.MetaTitle)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(translation => translation.MetaDescription)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode();

        builder.HasOne(translation => translation.Product)
            .WithMany(product => product.Translations)
            .HasForeignKey(translation => translation.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(translation => new { translation.ProductId, translation.Language })
            .IsUnique();

        builder.HasIndex(translation => new { translation.Language, translation.Slug })
            .IsUnique();
    }
}
