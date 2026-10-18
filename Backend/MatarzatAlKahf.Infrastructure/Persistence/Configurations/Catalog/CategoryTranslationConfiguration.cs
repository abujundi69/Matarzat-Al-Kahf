using MatarzatAlKahf.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Catalog;

public sealed class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.ToTable("CategoryTranslations", "catalog");
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

        builder.Property(translation => translation.Description)
            .IsRequired()
            .HasMaxLength(4000)
            .IsUnicode();

        builder.Property(translation => translation.MetaTitle)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(translation => translation.MetaDescription)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode();

        builder.HasOne(translation => translation.Category)
            .WithMany(category => category.Translations)
            .HasForeignKey(translation => translation.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(translation => new { translation.CategoryId, translation.Language })
            .IsUnique();

        builder.HasIndex(translation => new { translation.Language, translation.Slug })
            .IsUnique();
    }
}
