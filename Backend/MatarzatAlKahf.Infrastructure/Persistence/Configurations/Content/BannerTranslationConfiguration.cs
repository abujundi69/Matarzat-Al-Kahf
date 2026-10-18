using MatarzatAlKahf.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Content;

public sealed class BannerTranslationConfiguration : IEntityTypeConfiguration<BannerTranslation>
{
    public void Configure(EntityTypeBuilder<BannerTranslation> builder)
    {
        builder.ToTable("BannerTranslations", "content");
        builder.ConfigureBaseEntity();

        builder.Property(translation => translation.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(translation => translation.Title)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(translation => translation.DesktopImageUrl)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.Property(translation => translation.MobileImageUrl)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.Property(translation => translation.AltText)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode();

        builder.Property(translation => translation.LinkUrl)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.HasOne(translation => translation.Banner)
            .WithMany(banner => banner.Translations)
            .HasForeignKey(translation => translation.BannerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(translation => new { translation.BannerId, translation.Language })
            .IsUnique();
    }
}
