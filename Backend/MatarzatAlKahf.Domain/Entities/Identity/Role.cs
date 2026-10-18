using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.Identity;

public sealed class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? DisplayNameArabic { get; set; }

    public string? DisplayNameEnglish { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsSystemRole { get; set; }

    public ICollection<AdminUserRole> UserRoles { get; set; } = new List<AdminUserRole>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
