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
    private async Task<BootstrapAdminResult> BootstrapCoreAsync(BootstrapAdminRequest request, string normalizedPhoneNumber, CancellationToken cancellationToken)
    {
        AdminUser? existingAdmin = await _dbContext.AdminUsers.OrderBy(user => user.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        DateTime now = DateTime.UtcNow;
        Dictionary<string, Role> roles = await EnsureRolesAsync(now, cancellationToken);
        Dictionary<string, Permission> permissions = await EnsurePermissionsAsync(now, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await EnsureRolePermissionMappingsAsync(roles, permissions, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        if (existingAdmin is not null)
        {
            return new BootstrapAdminResult(false, existingAdmin.Id, "Administrative user already exists; system authorization data was verified.");
        }

        var adminUser = new AdminUser
        {
            FullName = request.FullName.Trim(),
            PhoneNumber = normalizedPhoneNumber,
            NormalizedPhoneNumber = normalizedPhoneNumber,
            Email = string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim().ToLowerInvariant(),
            Status = UserStatus.Active,
            PreferredLanguage = LanguageCode.Arabic
        };
        adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, request.Password);
        _dbContext.AdminUsers.Add(adminUser);
        _dbContext.AdminUserRoles.Add(new AdminUserRole { AdminUserId = adminUser.Id, RoleId = roles[SystemRoleNames.SuperAdmin].Id });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new BootstrapAdminResult(true, adminUser.Id, "Initial administrator created.");
    }
}
