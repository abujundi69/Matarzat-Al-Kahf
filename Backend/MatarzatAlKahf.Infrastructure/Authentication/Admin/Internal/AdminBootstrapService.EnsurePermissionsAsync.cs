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
    private async Task<Dictionary<string, Permission>> EnsurePermissionsAsync(DateTime now, CancellationToken cancellationToken)
    {
        string[] codes = RequiredPermissions.Select(seed => seed.Code).ToArray();
        List<Permission> existingPermissions = await _dbContext.Permissions.Where(permission => codes.Contains(permission.Code)).ToListAsync(cancellationToken);
        var permissions = existingPermissions.GroupBy(permission => permission.Code, StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        foreach (PermissionSeed seed in RequiredPermissions)
        {
            if (!permissions.TryGetValue(seed.Code, out Permission? permission))
            {
                permission = new Permission
                {
                    Code = seed.Code,
                    Name = seed.Name,
                    Description = seed.Description
                };
                _dbContext.Permissions.Add(permission);
                permissions[seed.Code] = permission;
                continue;
            }

            permission.Code = seed.Code;
            permission.Name = seed.Name;
            permission.Description = seed.Description;
            permission.IsDeleted = false;
            permission.UpdatedAtUtc = now;
        }

        return permissions;
    }
}
