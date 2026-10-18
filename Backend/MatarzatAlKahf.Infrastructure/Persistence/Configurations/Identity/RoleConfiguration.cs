using MatarzatAlKahf.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Identity;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", "identity");
        builder.ConfigureBaseEntity();

        builder.Property(role => role.Name)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(role => role.DisplayNameArabic)
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(role => role.DisplayNameEnglish)
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(role => role.Description)
            .IsRequired()
            .HasMaxLength(4000)
            .IsUnicode();

        builder.Property(role => role.IsSystemRole)
            .HasDefaultValue(false);

        builder.HasIndex(role => role.Name)
            .IsUnique();
    }
}
