using System.Data;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Authentication.Admin;
public sealed partial class AdminBootstrapService
{
    private async Task EnsureRolePermissionMappingsAsync(Dictionary<string, Role> roles, Dictionary<string, Permission> permissions, CancellationToken cancellationToken)
    {
        var assignments = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            [SystemRoleNames.SuperAdmin] = PermissionCodes.All,
            [SystemRoleNames.Admin] = PermissionCodes.All
        };
        Guid[] roleIds = roles.Values.Select(role => role.Id).ToArray();
        var existingMappings = await _dbContext.RolePermissions.Where(mapping => roleIds.Contains(mapping.RoleId)).Select(mapping => new { mapping.RoleId, mapping.PermissionId }).ToListAsync(cancellationToken);
        var existingKeys = existingMappings.Select(mapping => (mapping.RoleId, mapping.PermissionId)).ToHashSet();
        foreach ((string roleName, IReadOnlyList<string> permissionCodes)in assignments)
        {
            Role role = roles[roleName];
            foreach (string permissionCode in permissionCodes)
            {
                Permission permission = permissions[permissionCode];
                if (!existingKeys.Add((role.Id, permission.Id)))
                {
                    continue;
                }

                _dbContext.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            }
        }
    }
}
