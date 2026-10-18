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
    private IQueryable<Role> Query() => dbContext.Roles.Where(role => !role.IsDeleted).Include(role => role.UserRoles).ThenInclude(mapping => mapping.AdminUser).Include(role => role.RolePermissions).ThenInclude(mapping => mapping.Permission);
}
