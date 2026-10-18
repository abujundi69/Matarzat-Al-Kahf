using MatarzatAlKahf.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Identity;

public sealed class AdminUserRoleConfiguration : IEntityTypeConfiguration<AdminUserRole>
{
    public void Configure(EntityTypeBuilder<AdminUserRole> builder)
    {
        builder.ToTable("AdminUserRoles", "identity");
        builder.HasKey(adminUserRole => new { adminUserRole.AdminUserId, adminUserRole.RoleId });

        builder.HasOne(adminUserRole => adminUserRole.AdminUser)
            .WithMany(adminUser => adminUser.UserRoles)
            .HasForeignKey(adminUserRole => adminUserRole.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(adminUserRole => adminUserRole.Role)
            .WithMany(role => role.UserRoles)
            .HasForeignKey(adminUserRole => adminUserRole.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(adminUserRole => adminUserRole.RoleId);
    }
}
