using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.System;

public sealed class StoreSettingTranslationConfiguration : IEntityTypeConfiguration<StoreSettingTranslation>
{
    public void Configure(EntityTypeBuilder<StoreSettingTranslation> builder)
    {
        builder.ToTable("StoreSettingTranslations", "system");
        builder.ConfigureBaseEntity();

        builder.Property(translation => translation.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(translation => translation.Value)
            .IsRequired()
            .HasMaxLength(4000)
            .IsUnicode();

        builder.HasOne(translation => translation.StoreSetting)
            .WithMany(setting => setting.Translations)
            .HasForeignKey(translation => translation.StoreSettingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(translation => new { translation.StoreSettingId, translation.Language })
            .IsUnique();
    }
}
