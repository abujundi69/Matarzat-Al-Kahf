using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.System;

public sealed class SeoSettingConfiguration : IEntityTypeConfiguration<SeoSetting>
{
    public void Configure(EntityTypeBuilder<SeoSetting> builder)
    {
        builder.ToTable("SeoSettings", "system");
        builder.ConfigureBaseEntity();
        builder.Property(item => item.BaseUrl).IsRequired().HasMaxLength(1000).IsUnicode();
        builder.Property(item => item.ArabicTitle).IsRequired().HasMaxLength(300).IsUnicode();
        builder.Property(item => item.EnglishTitle).IsRequired().HasMaxLength(300).IsUnicode();
        builder.Property(item => item.ArabicDescription).IsRequired().HasMaxLength(1000).IsUnicode();
        builder.Property(item => item.EnglishDescription).IsRequired().HasMaxLength(1000).IsUnicode();
        builder.Property(item => item.ArabicOpenGraphImageUrl)
            .IsRequired().HasMaxLength(1000).IsUnicode();
        builder.Property(item => item.EnglishOpenGraphImageUrl)
            .IsRequired().HasMaxLength(1000).IsUnicode();
        builder.Property(item => item.RobotsAdditionalRules)
            .IsRequired().HasMaxLength(4000).IsUnicode();
    }
}
