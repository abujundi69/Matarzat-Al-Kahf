using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Requests;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.DTOs.Responses;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.RolesAndPermissions.Services;
public sealed partial class RoleService
{
    private static RoleResponse Map(Role role) => new(role.Id, role.Name, role.Description, role.IsSystemRole, role.UserRoles.Count(mapping => !mapping.AdminUser.IsDeleted), role.RolePermissions.Where(mapping => !mapping.Permission.IsDeleted).Select(mapping => new PermissionResponse(mapping.Permission.Id, mapping.Permission.Code, mapping.Permission.Name, mapping.Permission.Description)).OrderBy(permission => permission.Code).ToArray())
    {
        DisplayNameArabic = role.DisplayNameArabic,
        DisplayNameEnglish = role.DisplayNameEnglish
    };
}
