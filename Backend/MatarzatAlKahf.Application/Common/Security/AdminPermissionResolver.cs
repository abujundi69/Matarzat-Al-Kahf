using MatarzatAlKahf.Domain.Entities.Identity;

namespace MatarzatAlKahf.Application.Common.Security;

/// <summary>
/// Resolves the permissions an administrator actually holds from the stored role-permission
/// mappings. Authorization must never fall back to the full permission catalogue: the stored
/// rows are the single source of truth, so revoking a role or a mapping takes effect instead of
/// being silently overridden.
/// </summary>
public static class AdminPermissionResolver
{
    /// <summary>
    /// Requires <see cref="AdminUser.UserRoles"/> loaded with their roles, role permissions,
    /// and permissions. An administrator with no surviving mappings resolves to no permissions.
    /// </summary>
    public static string[] Resolve(AdminUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return user.UserRoles
            .Select(mapping => mapping.Role)
            .Where(role => role is not null && !role.IsDeleted)
            .SelectMany(role => role.RolePermissions)
            .Select(mapping => mapping.Permission)
            .Where(permission => permission is not null && !permission.IsDeleted)
            .Select(permission => permission.Code)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();
    }
}
