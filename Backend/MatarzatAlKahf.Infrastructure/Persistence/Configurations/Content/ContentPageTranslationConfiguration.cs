using MatarzatAlKahf.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Content;

public sealed class ContentPageTranslationConfiguration : IEntityTypeConfiguration<ContentPageTranslation>
{
    public void Configure(EntityTypeBuilder<ContentPageTranslation> builder)
    {
        builder.ToTable("ContentPageTranslations", "content");
        builder.ConfigureBaseEntity();

        builder.Property(translation => translation.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(translation => translation.Title)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(translation => translation.Slug)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(translation => translation.Content)
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

        builder.HasOne(translation => translation.ContentPage)
            .WithMany(page => page.Translations)
            .HasForeignKey(translation => translation.ContentPageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(translation => new { translation.ContentPageId, translation.Language })
            .IsUnique();

        builder.HasIndex(translation => new { translation.Language, translation.Slug })
            .IsUnique();
    }
}
