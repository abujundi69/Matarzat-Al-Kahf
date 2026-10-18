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
    public async Task<AdminUserResponse> UpdateAsync(Guid id, UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        AdminUser user = await FindUserAsync(id, true, cancellationToken);
        await EnsureMayMutateAsync(user, cancellationToken);
        ValidateIdentity(request.FullName, request.Email);
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = phoneNormalizer.Normalize(request.PhoneNumber);
        user.NormalizedPhoneNumber = user.PhoneNumber;
        user.Email = NormalizeEmail(request.Email);
        user.PreferredLanguage = request.PreferredLanguage;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await SaveConflictSafeAsync(cancellationToken);
        await AuditAsync("admin.user.updated", user, cancellationToken);
        return await GetAsync(id, cancellationToken);
    }
}
