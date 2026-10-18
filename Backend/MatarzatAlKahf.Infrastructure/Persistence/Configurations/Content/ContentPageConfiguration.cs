using MatarzatAlKahf.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Content;

public sealed class ContentPageConfiguration : IEntityTypeConfiguration<ContentPage>
{
    public void Configure(EntityTypeBuilder<ContentPage> builder)
    {
        builder.ToTable("ContentPages", "content");
        builder.ConfigureBaseEntity();

        builder.Property(page => page.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(page => page.IsPublished)
            .HasDefaultValue(false);

        builder.HasIndex(page => new { page.Type, page.IsPublished });
    }
}
