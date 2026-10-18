using MatarzatAlKahf.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Identity;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", "identity");
        builder.ConfigureBaseEntity();

        builder.Property(permission => permission.Code)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(permission => permission.Name)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(permission => permission.Description)
            .IsRequired()
            .HasMaxLength(4000)
            .IsUnicode();

        builder.HasIndex(permission => permission.Code)
            .IsUnique();
    }
}
