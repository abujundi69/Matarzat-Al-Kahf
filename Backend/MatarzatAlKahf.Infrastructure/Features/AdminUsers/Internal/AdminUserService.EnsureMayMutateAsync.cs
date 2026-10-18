using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.AdminUsers.Services;

public sealed partial class AdminUserService
{
    private const string ProtectedSuperAdminMessage =
        "A normal Admin cannot modify a SuperAdmin account.";

    private async Task EnsureMayMutateAsync(
        AdminUser target,
        CancellationToken cancellationToken)
    {
        if (!IsSuperAdmin(target))
        {
            return;
        }

        Guid actorId = currentActor.AdminUserId
            ?? throw new AuthenticationFailedException(
                "Administrator authentication is required.");
        bool actorIsSuperAdmin = actorId == target.Id
            || await dbContext.AdminUsers.AnyAsync(
                user => user.Id == actorId
                    && !user.IsDeleted
                    && user.UserRoles.Any(mapping =>
                        mapping.Role.Name == SystemRoleNames.SuperAdmin),
                cancellationToken);

        if (!actorIsSuperAdmin)
        {
            throw new AuthorizationDeniedException(ProtectedSuperAdminMessage);
        }
    }

    private static bool IsSuperAdmin(AdminUser user) =>
        user.UserRoles.Any(mapping =>
            mapping.Role.Name == SystemRoleNames.SuperAdmin);
}
