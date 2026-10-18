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
    public async Task<AdminUserResponse> CreateAsync(CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        ValidateIdentity(request.FullName, request.Email);
        passwordPolicy.Validate(request.Password);
        string phone = phoneNormalizer.Normalize(request.PhoneNumber);
        Role adminRole = await LoadAdministrativeRoleAsync(cancellationToken);
        var user = new AdminUser
        {
            FullName = request.FullName.Trim(),
            PhoneNumber = phone,
            NormalizedPhoneNumber = phone,
            Email = NormalizeEmail(request.Email),
            Status = UserStatus.Active,
            PreferredLanguage = request.PreferredLanguage
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        user.UserRoles.Add(new AdminUserRole
        {
            AdminUserId = user.Id,
            RoleId = adminRole.Id
        });

        dbContext.AdminUsers.Add(user);
        await SaveConflictSafeAsync(cancellationToken);
        await auditService.RecordAsync("admin.user.created", nameof(AdminUser), user.Id, $"Administrator '{user.FullName}' was created.", cancellationToken);
        return await GetAsync(user.Id, cancellationToken);
    }
}
