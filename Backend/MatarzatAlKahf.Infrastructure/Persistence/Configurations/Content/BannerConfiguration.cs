using MatarzatAlKahf.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Content;

public sealed class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("Banners", "content");
        builder.ConfigureBaseEntity();

        builder.Property(banner => banner.Position)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(banner => banner.SortOrder)
            .HasDefaultValue(0);

        builder.Property(banner => banner.IsActive)
            .HasDefaultValue(false);

        builder.HasIndex(banner => new { banner.IsActive, banner.Position, banner.SortOrder });
        builder.HasIndex(banner => new { banner.StartsAtUtc, banner.EndsAtUtc });
    }
}
