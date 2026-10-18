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
    private async Task<Dictionary<string, Role>> EnsureRolesAsync(DateTime now, CancellationToken cancellationToken)
    {
        string[] names = RequiredRoles.Select(seed => seed.Name).ToArray();
        List<Role> existingRoles = await _dbContext.Roles.Where(role => names.Contains(role.Name)).ToListAsync(cancellationToken);
        var roles = existingRoles.GroupBy(role => role.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        foreach (RoleSeed seed in RequiredRoles)
        {
            if (!roles.TryGetValue(seed.Name, out Role? role))
            {
                role = new Role
                {
                    Name = seed.Name,
                    Description = seed.Description,
                    DisplayNameArabic = seed.DisplayNameArabic,
                    DisplayNameEnglish = seed.DisplayNameEnglish,
                    IsSystemRole = true
                };
                _dbContext.Roles.Add(role);
                roles[seed.Name] = role;
                continue;
            }

            role.Name = seed.Name;
            role.Description = seed.Description;
            role.DisplayNameArabic = seed.DisplayNameArabic;
            role.DisplayNameEnglish = seed.DisplayNameEnglish;
            role.IsSystemRole = true;
            role.IsDeleted = false;
            role.UpdatedAtUtc = now;
        }

        return roles;
    }
}
