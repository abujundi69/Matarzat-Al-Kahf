using MatarzatAlKahf.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Content;

public sealed class SocialMediaLinkConfiguration : IEntityTypeConfiguration<SocialMediaLink>
{
    public void Configure(EntityTypeBuilder<SocialMediaLink> builder)
    {
        builder.ToTable("SocialMediaLinks", "content");
        builder.ConfigureBaseEntity();

        builder.Property(link => link.Platform)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(link => link.Url)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.Property(link => link.SortOrder)
            .HasDefaultValue(0);

        builder.Property(link => link.IsActive)
            .HasDefaultValue(false);

        builder.HasIndex(link => new { link.IsActive, link.SortOrder });
        builder.HasIndex(link => link.Platform);
    }
}
