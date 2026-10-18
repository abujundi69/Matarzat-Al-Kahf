using MatarzatAlKahf.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Identity;

public sealed class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("AdminUsers", "identity");
        builder.ConfigureBaseEntity();

        builder.Property(adminUser => adminUser.FullName)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(adminUser => adminUser.PhoneNumber)
            .IsRequired()
            .HasMaxLength(16)
            .IsUnicode();

        builder.Property(adminUser => adminUser.NormalizedPhoneNumber)
            .IsRequired()
            .HasMaxLength(16)
            .IsUnicode(false);

        builder.Property(adminUser => adminUser.Email)
            .HasMaxLength(320)
            .IsUnicode();

        builder.Property(adminUser => adminUser.PasswordHash)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode();

        builder.Property(adminUser => adminUser.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(adminUser => adminUser.PreferredLanguage)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.HasIndex(adminUser => adminUser.NormalizedPhoneNumber)
            .IsUnique();

        builder.HasIndex(adminUser => adminUser.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");
    }
}
