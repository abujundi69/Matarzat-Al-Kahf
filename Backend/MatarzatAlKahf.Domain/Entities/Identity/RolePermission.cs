using System.Diagnostics.CodeAnalysis;

namespace MatarzatAlKahf.Domain.Entities.Identity;

[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "RolePermission is the domain term required for the role-permission join entity.")]
public sealed class RolePermission
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
