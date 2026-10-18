using System.Diagnostics.CodeAnalysis;
using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.Identity;

[SuppressMessage(
    "Naming",
    "CA1711:Identifiers should not have incorrect suffix",
    Justification = "Permission is the domain term required by the authorization model.")]
public sealed class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
