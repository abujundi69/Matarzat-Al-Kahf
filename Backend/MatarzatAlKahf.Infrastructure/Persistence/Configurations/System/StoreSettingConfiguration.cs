using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.System;

public sealed class StoreSettingConfiguration : IEntityTypeConfiguration<StoreSetting>
{
    public void Configure(EntityTypeBuilder<StoreSetting> builder)
    {
        builder.ToTable("StoreSettings", "system");
        builder.ConfigureBaseEntity();

        builder.Property(setting => setting.Key)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(setting => setting.Value)
            .HasMaxLength(4000)
            .IsUnicode();

        builder.Property(setting => setting.Group)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(setting => setting.Description)
            .IsRequired()
            .HasMaxLength(4000)
            .IsUnicode();

        builder.Property(setting => setting.IsPublic)
            .HasDefaultValue(false);

        builder.Property(setting => setting.IsLocalized)
            .HasDefaultValue(false);

        builder.HasIndex(setting => setting.Key)
            .IsUnique();

        builder.HasIndex(setting => setting.Group);
    }
}
