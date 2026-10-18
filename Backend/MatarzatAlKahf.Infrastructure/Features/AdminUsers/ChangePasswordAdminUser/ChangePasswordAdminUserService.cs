using System.Net.Mail;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
using MatarzatAlKahf.Application.Features.AdminUsers.Interfaces;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.AdminUsers.Services;

public sealed partial class AdminUserService
{
    public async Task ChangePasswordAsync(ChangeAdminPasswordRequest request, CancellationToken cancellationToken)
    {
        AdminUser user = await GetCurrentAsync(cancellationToken);
        if (!passwordHasher.VerifyPassword(user, request.CurrentPassword))
        {
            throw new AuthenticationFailedException("Current password is invalid.");
        }

        passwordPolicy.Validate(request.NewPassword);
        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await adminSessionService.RevokeOthersAsync(
            user.Id,
            currentActor.SessionTokenId,
            cancellationToken);
        await AuditAsync("admin.profile.password.changed", user, cancellationToken);
    }
}
