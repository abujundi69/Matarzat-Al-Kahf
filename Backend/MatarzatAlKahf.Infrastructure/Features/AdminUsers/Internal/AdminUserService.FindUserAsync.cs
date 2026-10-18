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
    private async Task<AdminUser> FindUserAsync(Guid id, bool tracked, CancellationToken cancellationToken)
    {
        IQueryable<AdminUser> query = dbContext.AdminUsers.Where(user => user.Id == id && !user.IsDeleted).Include(user => user.UserRoles).ThenInclude(mapping => mapping.Role).ThenInclude(role => role.RolePermissions).ThenInclude(mapping => mapping.Permission);
        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Administrator was not found.");
    }
}
