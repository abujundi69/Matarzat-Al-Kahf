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
    public async Task ResetPasswordAsync(Guid id, ResetAdminPasswordRequest request, CancellationToken cancellationToken)
    {
        AdminUser user = await FindUserAsync(id, true, cancellationToken);
        await EnsureMayMutateAsync(user, cancellationToken);
        passwordPolicy.Validate(request.NewPassword);
        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await adminSessionService.RevokeAllAsync(user.Id, cancellationToken);
        await AuditAsync("admin.user.password.reset", user, cancellationToken);
    }
}
