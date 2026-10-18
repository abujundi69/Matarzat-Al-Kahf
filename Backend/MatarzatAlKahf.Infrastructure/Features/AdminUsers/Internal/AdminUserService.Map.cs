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
    private static AdminUserResponse Map(AdminUser user)
    {
        bool isSuperAdmin = IsSuperAdmin(user);
        string effectiveRoleName = isSuperAdmin
            ? SystemRoleNames.SuperAdmin
            : SystemRoleNames.Admin;
        Role? effectiveRole = user.UserRoles
            .Select(mapping => mapping.Role)
            .FirstOrDefault(role => role.Name == effectiveRoleName);
        AdminRoleSummaryResponse[] roles =
        [
            new AdminRoleSummaryResponse(effectiveRole?.Id ?? Guid.Empty, effectiveRoleName)
            {
                DisplayNameArabic = effectiveRole?.DisplayNameArabic
                    ?? (isSuperAdmin ? "مدير النظام الأعلى" : "مدير"),
                DisplayNameEnglish = effectiveRole?.DisplayNameEnglish
                    ?? effectiveRoleName
            }
        ];
        string[] permissions = AdminPermissionResolver.Resolve(user);
        return new AdminUserResponse(user.Id, user.FullName, user.PhoneNumber, user.Email, user.Status, user.PreferredLanguage, user.LastLoginAtUtc, user.CreatedAtUtc, roles, permissions);
    }
}
